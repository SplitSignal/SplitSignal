// Puzzle 1: the web maze. APIClient.js handles transport; this file owns maze UI.
(() => {
  function initialize() {
    const app = document.getElementById("mazeApp");
    if (!app) return;

    const grid = document.getElementById("mazeGrid");
    const title = document.getElementById("missionStatus");
    const detail = document.getElementById("missionDetail");
    const bar = document.getElementById("progressBar");
    const pct = document.getElementById("progressText");
    const clue = document.getElementById("clueText");
    const sendBtn = document.getElementById("sendSymbolButton");
    const unlockBtn = document.getElementById("demoUnlockButton");
    const toast = document.getElementById("toast");
    if (!grid) return;

    const map = [
      "111111111111111",
      "100000000000000",
      "101110111110101",
      "100010000010001",
      "111010111010111",
      "100000100000001",
      "101110101111101",
      "100010000000001",
      "101011111011101",
      "101000001000001",
      "101111101110101",
      "100000100000001",
      "111010111011101",
      "100000000000000",
      "111111111111111",
    ];
    const rows = map.length;
    const cols = map[0].length;
    const exit = { r: 13, c: 13 };
    const symbolList = [
      { id: "snowflake", label: "Snowflake", r: 13, c: 1, icon: "❄" },
      { id: "tree", label: "Tree", r: 1, c: 13, icon: "♠" },
      { id: "candle", label: "Candle", r: 7, c: 7, icon: "♟" },
    ];
    const symbolsById = new Map(symbolList.map((s) => [s.id, s]));
    const discovered = new Set();
    const collectedByUnity = new Set();
    const failedNotifications = new Set();
    const demoMode = sessionStorage.getItem("demoMode") === "true";
    let player = { r: 1, c: 1 };
    let exitVisible = false;
    let mazeComplete = false;
    let exitSubmitting = false;

    function activity(message) {
      const log = document.getElementById("activityLog");
      if (!log) return;
      const entry = document.createElement("p");
      entry.className = "activity-entry";
      entry.textContent = `${new Date().toLocaleTimeString()} — ${message}`;
      log.prepend(entry);
      while (log.children.length > 10) log.lastElementChild.remove();
    }

    function notify(message) {
      if (!toast) return;
      toast.textContent = message;
      toast.classList.add("show");
      setTimeout(() => toast.classList.remove("show"), 2400);
    }

    // Put the symbol status in the existing Mission Status card to the right
    // of the maze. Symbols do not appear here until the web player discovers them.
    const statusCard = title?.closest(".info-card") || app;
    const statusSection = document.createElement("div");
    statusSection.id = "unitySymbolStatus";
    statusSection.className = "unity-symbol-status";
    statusSection.hidden = true;
    statusSection.innerHTML = '<div class="section-kicker">SYMBOLS</div><ul class="unity-symbol-list" aria-live="polite"></ul>';
    statusCard.appendChild(statusSection);
    const symbolUl = statusSection.querySelector("ul");

    function drawSymbolStatuses() {
      statusSection.hidden = discovered.size === 0;
      symbolUl.replaceChildren();
      for (const symbol of symbolList) {
        if (!discovered.has(symbol.id)) continue;
        const li = document.createElement("li");
        const label = document.createElement("span");
        label.textContent = `${symbol.icon} ${symbol.label}`;
        const state = document.createElement("strong");
        state.textContent = collectedByUnity.has(symbol.id)
          ? "Collected by Unity ✓"
          : failedNotifications.has(symbol.id)
            ? "Not delivered — retry"
            : "Waiting for Unity";
        if (failedNotifications.has(symbol.id)) {
          li.title = "Click to retry sending to Unity";
          li.style.cursor = "pointer";
        }
        li.append(label, state);
        symbolUl.appendChild(li);
      }
    }

    function isWall(r, c) {
      return r < 0 || c < 0 || r >= rows || c >= cols || map[r][c] === "1";
    }

    function render() {
      grid.style.setProperty("--maze-cols", cols);
      grid.replaceChildren();
      for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
          const el = document.createElement("div");
          el.className = "maze-cell " + (isWall(r, c) ? "wall" : "path");
          if (r === player.r && c === player.c) {
            el.classList.add("player-cell");
            el.textContent = "●";
          } else {
            const symbol = symbolList.find((s) => s.r === r && s.c === c);
            if (symbol && discovered.has(symbol.id)) {
              el.classList.add("checkpoint-cell");
              el.textContent = symbol.icon;
            } else if (exitVisible && r === exit.r && c === exit.c) {
              el.classList.add("exit-cell");
              el.textContent = "⚑";
            }
          }
          grid.appendChild(el);
        }
      }
    }

    function updateStatus() {
      const n = mazeComplete ? 100 : exitVisible ? 85 : Math.round((discovered.size + collectedByUnity.size) / 6 * 75);
      if (bar) bar.style.width = n + "%";
      if (pct) pct.textContent = n + "%";
      if (title) title.textContent = mazeComplete ? "Maze completed!" : exitVisible ? "Exit revealed" : "Explore the maze";
      if (detail) detail.textContent = mazeComplete
        ? "The Unity cage has been opened."
        : exitVisible
          ? "All symbols collected by Unity. Reach the exit flag."
          : `${discovered.size}/3 found on web · ${collectedByUnity.size}/3 collected by Unity`;
      if (clue) clue.textContent = mazeComplete
        ? "The cipher puzzle is now available below."
        : exitVisible
          ? "The exit is open. Reach the flag."
          : "Find each symbol; your Unity partner must collect all three.";
      drawSymbolStatuses();
    }

    function checkExitUnlock() {
      if (exitVisible || !symbolList.every((s) => discovered.has(s.id) && collectedByUnity.has(s.id))) return;
      exitVisible = true;
      activity("Unity collected all three symbols. Maze exit revealed.");
      notify("All three symbols collected — exit unlocked!");
      render();
      updateStatus();
    }

    async function notifyUnity(symbol) {
      const ok = await window.UpdateReq(symbol.id, "Found", "Unity");
      if (!ok) {
        failedNotifications.add(symbol.id);
        activity(`Could not notify Unity about ${symbol.label}.`);
        notify(`Could not send ${symbol.label}. Click its status to retry.`);
      } else {
        failedNotifications.delete(symbol.id);
      }
      drawSymbolStatuses();
    }

    symbolUl.addEventListener("click", (event) => {
      const item = event.target.closest("li");
      if (!item) return;
      const index = [...symbolUl.children].indexOf(item);
      const id = symbolList.filter((s) => discovered.has(s.id))[index]?.id;
      if (id && failedNotifications.has(id)) void notifyUnity(symbolsById.get(id));
    });

    async function completeMaze() {
      if (!exitVisible || mazeComplete || exitSubmitting) return;
      exitSubmitting = true;
      activity("Exit reached. Sending cage open request to Unity.");
      const ok = await window.UpdateReq("cage", "open", "Unity");
      exitSubmitting = false;
      if (!ok) {
        activity("Cage request failed. Leave and re-enter the exit to retry.");
        notify("Cage request failed. Retry the exit.");
        return;
      }
      mazeComplete = true;
      updateStatus();
      window.dispatchEvent(new CustomEvent("split-signal-maze-complete"));
      activity("Cage open request sent. Cipher puzzle unlocked.");
    }

    function move(direction) {
      if (mazeComplete || exitSubmitting) return;
      const delta = { up: [-1, 0], down: [1, 0], left: [0, -1], right: [0, 1] }[direction];
      if (!delta) return;
      const r = player.r + delta[0];
      const c = player.c + delta[1];
      if (isWall(r, c)) { notify("That way is blocked."); return; }
      if (r === exit.r && c === exit.c && !exitVisible) {
        notify("The exit is locked until Unity collects every symbol.");
        return;
      }
      player = { r, c };
      const symbol = symbolList.find((s) => s.r === r && s.c === c && !discovered.has(s.id));
      if (symbol) {
        discovered.add(symbol.id);
        activity(`${symbol.label} discovered in the web maze.`);
        notify(`Found ${symbol.label}. Sending update to Unity...`);
        void notifyUnity(symbol);
      }
      render();
      updateStatus();
      if (r === exit.r && c === exit.c) void completeMaze();
    }

    window.addEventListener("split-signal-update", (event) => {
      const name = String(event.detail?.object ?? "").trim().toLowerCase();
      const state = String(event.detail?.status ?? "").trim().toLowerCase();
      // Unity MUST send Collected; Web sends Found when first discovering it.
      if (!symbolsById.has(name) || state !== "collected") return;
      if (collectedByUnity.has(name)) return;
      collectedByUnity.add(name);
      activity(`Unity collected ${symbolsById.get(name).label}.`);
      updateStatus();
      checkExitUnlock();
    });

    // Discovery sends immediately. Hide legacy manual-send UI so it cannot
    // produce a duplicate or incorrect Paraglider update.
    if (sendBtn) { sendBtn.hidden = true; sendBtn.classList.add("hidden"); }

    // Optional local demo behavior; live mode can ONLY unlock by Unity messages.
    if (unlockBtn) {
      unlockBtn.textContent = "Demo: simulate Unity collecting symbols";
      unlockBtn.classList.toggle("hidden", !demoMode);
      unlockBtn.hidden = !demoMode;
      unlockBtn.addEventListener("click", () => {
        if (!demoMode) return;
        for (const symbol of symbolList) {
          if (!discovered.has(symbol.id)) continue;
          window.dispatchEvent(new CustomEvent("split-signal-update", {
            detail: { object: symbol.id, status: "Collected" }
          }));
        }
      });
    }

    document.querySelectorAll("[data-move]").forEach((button) => {
      button.addEventListener("click", () => move(button.dataset.move));
    });
    document.addEventListener("keydown", (event) => {
      if (["INPUT", "TEXTAREA"].includes(document.activeElement?.tagName)) return;
      const keys = {
        ArrowUp: "up", w: "up", ArrowDown: "down", s: "down",
        ArrowLeft: "left", a: "left", ArrowRight: "right", d: "right"
      };
      const direction = keys[event.key] || keys[event.key.toLowerCase()];
      if (direction) { event.preventDefault(); move(direction); }
    });

    render();
    updateStatus();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else initialize();
})();
