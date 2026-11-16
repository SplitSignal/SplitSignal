(() => {
  const app = document.getElementById('coopPuzzleConsole');
  if (!app) return;
  const root = document.getElementById('mazeApp');
  const sessionId = root?.dataset.sessionId || 'DEMO-SESSION';
  const symbols = new Set();
  const expectedCipher = 'EZIRLNVGVI'; // Atbash ciphertext for VARIOMETER
  const expectedWord = 'VARIOMETER';
  const decodeForm = document.getElementById('decodeForm');
  const decodedWord = document.getElementById('decodedWord');
  const decodeFeedback = document.getElementById('decodeFeedback');
  const cageButton = document.getElementById('cageButton');
  const pedestalButton = document.getElementById('pedestalButton');
  const sequenceFeedback = document.getElementById('sequenceFeedback');
  const cubeGrid = document.getElementById('cubeGrid');
  const cubeStatus = document.getElementById('cubeStatus');
  const cubeFeedback = document.getElementById('cubeFeedback');
  const freezeCubes = document.getElementById('freezeCubes');
  const resetCubes = document.getElementById('resetCubes');
  let decoded = false, cageOpened = false, cageUsed = false, escapeComplete = false;
  const cubeCount = 20, meltAfterMs = 5000;
  let cubes = [];
  let freezeStartedAt = 0;

  function log(message) {
    const activity = document.getElementById('activityLog');
    if (!activity) return;
    const entry = document.createElement('p');
    entry.className = 'activity-entry';
    entry.textContent = `${new Date().toLocaleTimeString()} — ${message}`;
    activity.prepend(entry);
    while (activity.children.length > 10) activity.lastElementChild.remove();
  }
  async function sendAction(actionId, objectId, value) {
    try {
      const response = await fetch('/api-proxy/game-action', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ actionId, puzzleId: 'WINTER-MAZE', sessionId, objectId, value })
      });
      const body = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(body.detail || body.message || 'Action rejected');
      log(body.message || `Action recorded: ${actionId}`);
      return true;
    } catch (error) {
      // The web demo should remain playable if the optional API proxy is unavailable.
      log(`Local puzzle event: ${actionId} (${error.message})`);
      return false;
    }
  }
  function updateSymbols() {
    document.querySelectorAll('.symbol-find').forEach(button => {
      const found = symbols.has(button.dataset.symbol);
      button.disabled = found;
      button.textContent = found ? 'Found ✓' : `Find ${button.dataset.symbol.toLowerCase()}`;
    });
    const allFound = symbols.size === 3;
    cageButton.disabled = !allFound || cageUsed;
    sequenceFeedback.textContent = escapeComplete ? 'Escape sequence complete. Pedestal dropped; proceed through the opening.' : cageUsed ? 'Caged button activated after decoding. Unity player can activate the paraglider.' : cageOpened && !decoded ? 'Cage is open. Decode VARIOMETER, then activate the button.' : allFound ? 'All symbols found. The cage is unlocked.' : `Find all three symbols (${symbols.size}/3) to unlock the cage.`;
    if (!decoded) decodeFeedback.textContent = allFound ? 'All symbols found. Enter the Atbash-decoded word.' : `Find all three symbols to open this stage (${symbols.size}/3).`;
  }
  document.querySelectorAll('.symbol-find').forEach(button => button.addEventListener('click', async () => {
    const symbol = button.dataset.symbol;
    if (symbols.has(symbol)) return;
    symbols.add(symbol);
    log(`Web player discovered ${symbol}.`);
    await sendAction('SYMBOL-FOUND', symbol.toLowerCase(), symbol);
    updateSymbols();
  }));
  // The maze emits this event when the player physically reaches a hidden symbol tile.
  window.addEventListener('coop-symbol-found', async event => {
    const symbol = String(event.detail?.symbol || '').toUpperCase();
    if (!['SNOWFLAKE', 'TREE', 'CANDLE'].includes(symbol) || symbols.has(symbol)) return;
    symbols.add(symbol);
    log(`Web player discovered ${symbol} in the maze.`);
    await sendAction('SYMBOL-FOUND', symbol.toLowerCase(), symbol);
    updateSymbols();
  });
  decodeForm.addEventListener('submit', async event => {
    event.preventDefault();
    if (symbols.size !== 3) { decodeFeedback.textContent = 'Find the snowflake, tree, and candle first.'; return; }
    const answer = decodedWord.value.trim().toUpperCase();
    if (answer !== expectedWord) {
      decodeFeedback.textContent = 'Not quite. Apply Atbash: A↔Z, B↔Y, and so on.';
      decodedWord.setAttribute('aria-invalid', 'true');
      return;
    }
    decoded = true;
    decodedWord.removeAttribute('aria-invalid');
    decodeFeedback.textContent = 'Correct — VARIOMETER decoded. The cage mechanism is ready.';
    if (cageOpened && !cageUsed) { cageButton.textContent = 'Activate caged button'; cageButton.disabled = false; }
    cubeFeedback.textContent = 'Click cubes to place them. Keep the frozen-lake toggle on to pause every melt timer.';
    pedestalButton.disabled = true;
    log('Puzzle 2 solved: Atbash word VARIOMETER decoded.');
    await sendAction('ATBASH-DECODE-COMPLETE', 'variometer', expectedWord);
    updateCubeStatus();
  });
  cageButton.addEventListener('click', async () => {
    if (symbols.size !== 3 || cageUsed) return;
    if (!cageOpened) {
      cageOpened = true;
      cageButton.textContent = decoded ? 'Activate caged button' : 'Cage opened — decode word next';
      log('Cage opened after Puzzle 1.');
      await sendAction('CAGE-OPENED', 'caged_button_cage', 'OPEN');
      if (!decoded) { updateSymbols(); return; }
    }
    if (!decoded) { sequenceFeedback.textContent = 'Decode VARIOMETER first; then click this button again.'; updateSymbols(); return; }
    cageUsed = true;
    cageButton.textContent = 'Caged button activated ✓';
    cageButton.disabled = true;
    log('Caged button activated after Atbash decoding.');
    await sendAction('CAGED-BUTTON-ACTIVATE', 'caged_button', 'DECODED');
    updateSymbols();
  });

  function renderCubes() {
    cubeGrid.innerHTML = '';
    cubes.forEach((cube, index) => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = `ice-cube${cube.placed ? ' placed' : ''}${cube.melted ? ' melted' : ''}`;
      button.setAttribute('aria-label', `Cube ${index + 1}${cube.placed ? ', placed' : cube.melted ? ', melted' : ', available'}`);
      button.title = `Cube ${index + 1}`;
      button.disabled = !decoded || cube.placed || cube.melted || escapeComplete;
      button.textContent = cube.placed ? '◆' : cube.melted ? '×' : '';
      button.addEventListener('click', () => {
        if (!decoded || cube.placed || cube.melted || escapeComplete) return;
        cube.placed = true;
        cube.remaining = meltAfterMs;
        cube.startedAt = freezeCubes.checked ? 0 : Date.now();
        cube.deadlineAt = cube.startedAt ? cube.startedAt + cube.remaining : 0;
        log(`Ice image cube ${index + 1} placed.`);
        renderCubes(); updateCubeStatus();
        if (cubes.every(item => item.placed)) {
          cubeFeedback.textContent = 'Image complete! The Unity player can now trigger the pedestal drop.';
          pedestalButton.disabled = !cageUsed;
          sendAction('ICE-IMAGE-COMPLETE', 'ice_image', '20/20');
        }
      });
      cubeGrid.appendChild(button);
    });
  }
  function updateCubeStatus() {
    const placed = cubes.filter(c => c.placed).length;
    cubeStatus.textContent = `${placed} / ${cubeCount} cubes placed${freezeCubes.checked ? ' · timers paused' : ''}`;
    if (!decoded) cubeFeedback.textContent = 'Complete the Atbash clue first.';
    renderCubes();
    pedestalButton.disabled = !(decoded && cageUsed && placed === cubeCount) || escapeComplete;
  }
  function resetCubeState() {
    cubes = Array.from({ length: cubeCount }, () => ({ placed: false, melted: false, remaining: meltAfterMs, startedAt: 0, deadlineAt: 0 }));
    renderCubes(); updateCubeStatus();
  }
  freezeCubes.addEventListener('change', async () => {
    const now = Date.now();
    if (freezeCubes.checked) {
      freezeStartedAt = now;
      cubes.forEach(cube => {
        if (cube.placed && !cube.melted && cube.startedAt) {
          cube.remaining = Math.max(0, cube.deadlineAt - now);
          cube.startedAt = 0;
          cube.deadlineAt = 0;
        }
      });
      cubeFeedback.textContent = 'Frozen lake active — all cube timers are paused.';
    } else {
      const pausedFor = freezeStartedAt ? now - freezeStartedAt : 0;
      cubes.forEach(cube => { if (cube.placed && !cube.melted && cube.remaining > 0) { cube.startedAt = now; cube.deadlineAt = now + cube.remaining; } });
      freezeStartedAt = 0;
      cubeFeedback.textContent = `Frozen lake inactive. Timers resumed after ${Math.round(pausedFor / 1000)}s.`;
    }
    await sendAction('FROZEN-LAKE-STATE', 'frozen_lake', freezeCubes.checked ? 'ACTIVE' : 'INACTIVE');
    updateCubeStatus();
  });
  resetCubes.addEventListener('click', () => { resetCubeState(); cubeFeedback.textContent = decoded ? 'Cube image reset. Place each cube and keep the lake frozen.' : 'Complete the Atbash clue first.'; });
  pedestalButton.addEventListener('click', async () => {
    if (!decoded || !cageUsed || !cubes.every(c => c.placed) || escapeComplete) return;
    escapeComplete = true;
    pedestalButton.textContent = 'Pedestal dropped ✓';
    pedestalButton.disabled = true;
    cubeFeedback.textContent = 'All 20 cubes are in place. Escape sequence complete.';
    sequenceFeedback.textContent = 'The pedestal vanishes. The opening is clear — drop through to escape.';
    log('Escape sequence completed: pedestal dropped and exit opened.');
    await sendAction('PEDESTAL-DROP-COMPLETE', 'pedestal', 'ESCAPED');
    renderCubes();
  });
  // A 100ms tick keeps each cube's five-second timer independent. Frozen-lake state pauses all.
  window.setInterval(() => {
    if (freezeCubes.checked || escapeComplete) return;
    let changed = false;
    const now = Date.now();
    cubes.forEach(cube => {
      if (!cube.placed || cube.melted || !cube.startedAt) return;
      cube.remaining = cube.deadlineAt - now;
      if (cube.remaining <= 0) { cube.melted = true; cube.placed = false; cube.startedAt = 0; cube.deadlineAt = 0; changed = true; }
    });
    if (changed) {
      cubeFeedback.textContent = 'Some cubes melted. Reset the image and try again with the frozen lake active.';
      log('An ice cube melted because its five-second timer expired.');
      updateCubeStatus();
    }
  }, 100);

  // Surface cipher metadata for test automation and keep the displayed clue canonical.
  document.getElementById('cipherText').textContent = expectedCipher;
  resetCubeState();
  updateSymbols();
})();
