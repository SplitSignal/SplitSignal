// Puzzle 2 + 3. All external messages go through APIClient.js.
(() => {
  function initialize() {
    const consoleSection = document.getElementById("coopPuzzleConsole");
    if (!consoleSection) return;
    const decodeStage = document.getElementById("decodeStage");
    const decodeForm = document.getElementById("decodeForm");
    const decodedWord = document.getElementById("decodedWord");
    const decodeFeedback = document.getElementById("decodeFeedback");
    const cipherText = document.getElementById("cipherText");
    const cubeGrid = document.getElementById("cubeGrid");
    const cubeStatus = document.getElementById("cubeStatus");
    const cubeFeedback = document.getElementById("cubeFeedback");
    const freezeCubes = document.getElementById("freezeCubes");
    const resetCubes = document.getElementById("resetCubes");
    const finalStage = consoleSection.querySelector(".final-stage");
    const sequenceFeedback = document.getElementById("sequenceFeedback");
    const pedestalButton = document.getElementById("pedestalButton");
    const cageButton = document.getElementById("cageButton");
    if (!decodeStage || !decodeForm || !decodedWord || !cubeGrid || !freezeCubes) return;

    const iceStage = cubeGrid.closest(".puzzle-stage");
    const demoMode = sessionStorage.getItem("demoMode") === "true";
    const expectedWord = "VARIOMETER";
    const expectedCipher = "EZIRLNVGVI";
    const meltAfterMs = 5000;

    // ILLUSTRATIVE 6 x 6 pedestal. Replace these rows with the shape you
    // actually want Unity to show; '#' means a required block, '.' empty.
    // This pattern uses 20 blocks, matching the prior 20-cube requirement.
    const targetShape = [
      "..##..",
      "..##..",
      ".####.",
      ".####.",
      "######",
      "..##..",
    ];
    const shapeCols = targetShape[0].length;
    const targetKeys = new Set();
    for (let r = 0; r < targetShape.length; r++) {
      for (let c = 0; c < shapeCols; c++) {
        if (targetShape[r][c] === "#") targetKeys.add(`${r},${c}`);
      }
    }

    let mazeComplete = false;
    let decoded = false;
    let submittingCode = false;
    let diagramFound = false;
    let icePondFrozen = false;
    let sendingPedestal = false;
    let pedestalDropped = false;
    let clueReceived = false;
    let blocks = targetShape.flatMap((row, r) => [...row].map((_, c) => ({
      key: `${r},${c}`, placed: false, melted: false,
      remainingMs: meltAfterMs, deadlineAt: 0
    })));

    function log(message) {
      const activity = document.getElementById("activityLog");
      if (!activity) return;
      const p = document.createElement("p");
      p.className = "activity-entry";
      p.textContent = `${new Date().toLocaleTimeString()} — ${message}`;
      activity.prepend(p);
      while (activity.children.length > 10) activity.lastElementChild.remove();
    }

    // Hide the old "Find symbol" buttons: web symbols are found by walking
    // through the maze now, not by clicking a second list.
    const legacySymbols = consoleSection.querySelector(".symbol-grid");
    if (legacySymbols) { legacySymbols.hidden = true; legacySymbols.classList.add("hidden"); }
    if (cageButton) { cageButton.hidden = true; cageButton.classList.add("hidden"); }
    const headingDescription = consoleSection.querySelector(".puzzle-heading p");
    if (headingDescription) headingDescription.textContent = "Decode Unity's clue, then assemble the pedestal before the ice melts.";

    // Never display the cipher rule before Unity sends code/Atbash cipher.
    if (cipherText) {
      cipherText.textContent = expectedCipher;
      const paragraph = cipherText.closest("p");
      if (paragraph) paragraph.replaceChildren(document.createTextNode("Encoded word: "), cipherText);
    }
    const intro = decodeStage.querySelector("div");
    const helpPrompt = document.createElement("p");
    helpPrompt.textContent = "Need help? Ask your partner to find a clue.";
    const unityClue = document.createElement("p");
    unityClue.id = "unityCipherClue";
    unityClue.className = "unity-cipher-clue";
    unityClue.hidden = true;
    intro?.append(helpPrompt, unityClue);

    const iceIntro = iceStage?.querySelector("p");
    if (iceIntro) iceIntro.textContent = "Copy the pedestal diagram. Each placed block melts in five seconds unless Unity is in the ice pond.";

    const referenceBox = document.createElement("div");
    referenceBox.className = "pedestal-reference-container";
    referenceBox.innerHTML = '<div class="section-kicker">PEDESTAL REFERENCE</div><div class="pedestal-reference" aria-label="Target pedestal pattern"></div>';
    const referenceGrid = referenceBox.querySelector(".pedestal-reference");
    referenceGrid.style.gridTemplateColumns = `repeat(${shapeCols}, minmax(0, 1fr))`;
    for (const row of targetShape) {
      for (const cell of row) {
        const square = document.createElement("span");
        square.className = cell === "#" ? "required" : "empty";
        referenceGrid.appendChild(square);
      }
    }
    cubeGrid.before(referenceBox);
    cubeGrid.style.gridTemplateColumns = `repeat(${shapeCols}, minmax(0, 1fr))`;

    const pondLabel = freezeCubes.closest("label");
    if (pondLabel) pondLabel.lastChild.textContent = demoMode ? " Demo: ice pond active" : " Unity in ice pond";
    freezeCubes.disabled = !demoMode; // Status display only in live mode.

    // Stages begin locked. The maze sends the completion event.
    consoleSection.hidden = true;
    if (iceStage) iceStage.hidden = true;
    if (finalStage) finalStage.hidden = true;
    if (pedestalButton) pedestalButton.disabled = true;

    function refreshCubeStatus() {
      const placed = blocks.filter((b) => b.placed).length;
      if (cubeStatus) cubeStatus.textContent = `${placed} / ${targetKeys.size} blocks placed${icePondFrozen ? " · timers paused" : " · melting active"}`;
    }

    function shapeIsCorrect() {
      return blocks.filter((b) => b.placed).length === targetKeys.size &&
        blocks.every((b) => b.placed === targetKeys.has(b.key));
    }

    function drawCubes() {
      cubeGrid.replaceChildren();
      blocks.forEach((cube, index) => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = `ice-cube${cube.placed ? " placed" : ""}${cube.melted ? " melted" : ""}`;
        button.title = `Position ${cube.key}`;
        button.setAttribute("aria-label", `Block ${index + 1}, ${cube.placed ? "placed" : "empty"}`);
        button.textContent = cube.placed ? "◆" : cube.melted ? "×" : "";
        button.disabled = !mazeComplete || !decoded || !diagramFound || pedestalDropped || sendingPedestal;
        button.addEventListener("click", () => {
          if (button.disabled) return;
          cube.placed = !cube.placed; // Click again to correct an incorrect placement.
          cube.melted = false;
          cube.remainingMs = meltAfterMs;
          cube.deadlineAt = cube.placed && !icePondFrozen ? Date.now() + meltAfterMs : 0;
          drawCubes();
          refreshCubeStatus();
          if (shapeIsCorrect()) void sendPedestalDrop();
        });
        cubeGrid.appendChild(button);
      });
    }

    function setPondFrozen(nextFrozen) {
      if (icePondFrozen === nextFrozen) return; // Unity only sends on transitions.
      const now = Date.now();
      icePondFrozen = nextFrozen;
      for (const cube of blocks) {
        if (!cube.placed) continue;
        if (nextFrozen) {
          cube.remainingMs = Math.max(0, cube.deadlineAt - now);
          cube.deadlineAt = 0;
        } else {
          cube.deadlineAt = now + cube.remainingMs;
        }
      }
      freezeCubes.checked = nextFrozen;
      if (cubeFeedback) cubeFeedback.textContent = nextFrozen
        ? "Unity entered the ice pond — block timers paused."
        : "Unity left the ice pond — block timers resumed.";
      log(nextFrozen ? "Ice pond active; timers paused." : "Ice pond inactive; timers resumed.");
      refreshCubeStatus();
    }

    async function sendPedestalDrop() {
      if (pedestalDropped || sendingPedestal || !shapeIsCorrect()) return;
      sendingPedestal = true;
      if (cubeFeedback) cubeFeedback.textContent = "Correct shape! Notifying Unity to drop the pedestal...";
      drawCubes();
      const ok = await window.UpdateReq("pedestal", "drop", "Unity");
      sendingPedestal = false;
      if (!ok) {
        if (cubeFeedback) cubeFeedback.textContent = "Correct shape, but Unity was not notified. Retry below.";
        if (finalStage) finalStage.hidden = false;
        if (pedestalButton) { pedestalButton.disabled = false; pedestalButton.textContent = "Retry pedestal drop"; }
        drawCubes();
        return;
      }
      pedestalDropped = true;
      if (cubeFeedback) cubeFeedback.textContent = "Pedestal shape completed! Drop request sent to Unity.";
      if (sequenceFeedback) sequenceFeedback.textContent = "Pedestal drop sent. Unity can continue the escape.";
      if (finalStage) finalStage.hidden = false;
      if (pedestalButton) { pedestalButton.textContent = "Pedestal drop sent ✓"; pedestalButton.disabled = true; }
      log("Pedestal drop sent to Unity.");
      drawCubes();
    }

    // Puzzle 1 must complete before the encoded word is available.
    window.addEventListener("split-signal-maze-complete", () => {
      if (mazeComplete) return;
      mazeComplete = true;
      consoleSection.hidden = false;
      if (decodeFeedback) decodeFeedback.textContent = "Decode the word. Ask Unity to find the clue if needed.";
      log("Maze complete. Cipher puzzle available.");
      maybeRevealIceStage();
    });

    function maybeRevealIceStage() {
      if (!mazeComplete || !decoded || !diagramFound) return;
      if (iceStage) iceStage.hidden = false;
      if (cubeFeedback) cubeFeedback.textContent = "Copy the pedestal reference while Unity freezes the ice pond.";
      drawCubes();
      refreshCubeStatus();
    }

    window.addEventListener("split-signal-update", (event) => {
      const object = String(event.detail?.object ?? "").trim().toLowerCase();
      const status = String(event.detail?.status ?? "").trim().toLowerCase();
      if (object === "code" && status === "atbash cipher") {
        if (!clueReceived) {
          clueReceived = true;
          unityClue.textContent = "Clue from Unity: Atbash cipher (A ↔ Z, B ↔ Y, C ↔ X).";
          unityClue.hidden = false;
          log("Unity sent the Atbash cipher clue.");
        }
      } else if (object === "icediagram" && status === "found") {
        if (!diagramFound) {
          diagramFound = true;
          log("Unity discovered the ice diagram.");
          maybeRevealIceStage();
        }
      } else if (object === "icepond" && (status === "true" || status === "false")) {
        setPondFrozen(status === "true");
      }
    });

    decodeForm.addEventListener("submit", async (event) => {
      event.preventDefault();
      if (!mazeComplete || decoded || submittingCode) return;
      const answer = decodedWord.value.trim().toUpperCase();
      if (answer !== expectedWord) {
        if (decodeFeedback) decodeFeedback.textContent = "Incorrect decoded word. Ask Unity for a clue.";
        decodedWord.setAttribute("aria-invalid", "true");
        return;
      }
      decodedWord.removeAttribute("aria-invalid");
      submittingCode = true;
      if (decodeFeedback) decodeFeedback.textContent = "Correct. Sending paraglider activation to Unity...";
      const ok = await window.UpdateReq("paraglider", "active", "Unity");
      submittingCode = false;
      if (!ok) {
        if (decodeFeedback) decodeFeedback.textContent = "Correct word, but notification failed. Submit again to retry.";
        return;
      }
      decoded = true;
      decodedWord.disabled = true;
      const submit = decodeForm.querySelector('button[type="submit"]');
      if (submit) submit.disabled = true;
      if (decodeFeedback) decodeFeedback.textContent = "Correct! Paraglider activated. Waiting for Unity to find the ice diagram.";
      log("VARIOMETER decoded. Paraglider activation sent to Unity.");
      maybeRevealIceStage();
    });

    if (demoMode) {
      freezeCubes.addEventListener("change", () => setPondFrozen(freezeCubes.checked));
    }
    resetCubes?.addEventListener("click", () => {
      if (!diagramFound || pedestalDropped) return;
      blocks = blocks.map((b) => ({ ...b, placed: false, melted: false, remainingMs: meltAfterMs, deadlineAt: 0 }));
      if (cubeFeedback) cubeFeedback.textContent = "Pedestal reset. Build the shape again.";
      drawCubes();
      refreshCubeStatus();
    });
    pedestalButton?.addEventListener("click", () => { if (shapeIsCorrect()) void sendPedestalDrop(); });

    // Five-second countdowns run only for blocks already placed.
    window.setInterval(() => {
      if (!diagramFound || icePondFrozen || pedestalDropped || sendingPedestal) return;
      const now = Date.now();
      let melted = false;
      for (const cube of blocks) {
        if (!cube.placed || !cube.deadlineAt || now < cube.deadlineAt) continue;
        cube.placed = false;
        cube.melted = true;
        cube.remainingMs = meltAfterMs;
        cube.deadlineAt = 0;
        melted = true;
      }
      if (melted) {
        if (cubeFeedback) cubeFeedback.textContent = "Some ice blocks melted. Replace them before their timers expire.";
        drawCubes();
        refreshCubeStatus();
      }
    }, 100);

    drawCubes();
    refreshCubeStatus();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else initialize();
})();
