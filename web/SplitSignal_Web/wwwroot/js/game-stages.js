// Visual stage progression only. APIClient.js continues to manage SignalR;
// maze.js and coop-puzzles.js continue to own the game rules and API requests.
(() => {
  function initialize() {
    const game = document.getElementById("mazeApp");
    if (!game) return;

    document.body.classList.add("split-signal-game");

    const stages = ["maze", "cipher", "ice", "complete"];
    const stageElements = new Map(stages.map(name => [name, document.getElementById(`stage-${name}`)]));
    const heading = document.getElementById("gameStageHeading");
    const description = document.getElementById("gameStageDescription");
    const stepItems = Array.from(document.querySelectorAll(".stage-step"));
    const titles = {
      maze: ["Winter Maze", "Find the three symbols. Unity must collect them before the exit opens."],
      cipher: ["The Cipher", "Decode the word and ask your partner to find the clue."],
      ice: ["Frozen Diagram", "Reconstruct the pedestal before the ice melts."],
      complete: ["Mission Complete", "Pedestal drop requested. Wait for your Unity teammate."]
    };

    let activeIndex = 0;

    function showStage(name) {
      const index = stages.indexOf(name);
      // Never allow navigation backwards or skipping a stage.
      if (index < 0 || index !== activeIndex + 1) return;
      activeIndex = index;
      renderStage(name);
    }

    function renderStage(name) {
      for (const [stageName, section] of stageElements) {
        if (!section) continue;
        const active = stageName === name;
        section.hidden = !active;
        section.inert = !active;
        section.classList.toggle("is-entering", active);
      }
      if (heading) heading.textContent = titles[name][0];
      if (description) description.textContent = titles[name][1];
      for (const item of stepItems) {
        const index = stages.indexOf(item.dataset.step);
        item.classList.toggle("is-done", index < activeIndex);
        item.classList.toggle("is-current", index === activeIndex);
        item.classList.toggle("is-locked", index > activeIndex);
        if (index === activeIndex) item.setAttribute("aria-current", "step");
        else item.removeAttribute("aria-current");
      }
      const activeSection = stageElements.get(name);
      if (activeSection) activeSection.scrollTop = 0;
    }

    // These events are emitted ONLY by the respective scripts when their
    // existing success conditions are fulfilled; this controller does no networking.
    window.addEventListener("split-signal-maze-complete", () => showStage("cipher"));
    window.addEventListener("split-signal-ice-ready", () => showStage("ice"));
    window.addEventListener("split-signal-game-complete", () => showStage("complete"));

    renderStage("maze");
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initialize, { once: true });
  } else {
    initialize();
  }
})();
