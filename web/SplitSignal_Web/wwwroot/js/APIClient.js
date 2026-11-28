// API / SignalR locations
const API_PATH = "https://split-signal-server-6235.onrender.com/game/";
const SIGNAL_PATH = "https://split-signal-server-6235.onrender.com/gamehub";

//const API_PATH = "http://localhost:33000/game/";
//const SIGNAL_PATH = "http://localhost:33000/gamehub";


// SignalR connection
const conn = new signalR.HubConnectionBuilder()
    .withUrl(SIGNAL_PATH)
    .withAutomaticReconnect()
    .build();


// ---------------------------------------
// SignalR Messages
// ---------------------------------------

// The API sends each update using SendAsync(req.Object, msg), which means
// req.Object is the SignalR EVENT NAME, not just a field in TestMessage.
// Unity must send the object names below in its UpdateReq requests.
const unityMessageNames = [
    "snowflake",
    "tree",
    "candle",
    "code",
    "iceDiagram",
    "icePond",
    "game"
];

for (const eventName of unityMessageNames) {
    conn.on(eventName, (msg) => {
        console.log(`SignalR ${eventName} received:`, msg);
        handleUpdate(msg, eventName);
    });
}

// Optional compatibility with any older testing messages.
conn.on("TestMessage", (msg) => {
    console.log("SignalR TestMessage received:", msg);
    handleUpdate(msg);
});

// Preserve the project's pre-existing named Paraglider UI listener.
conn.on("Paraglider", (msg) => {
    console.log("SignalR Paraglider received:", msg);
    const status = document.getElementById("puzzleStatus");
    if (status) status.textContent = msg?.status ?? msg?.Status ?? "";
    handleUpdate(msg, "Paraglider");
});

// One event bridge for all pages. Both maze.js and coop-puzzles.js listen for
// split-signal-update and use object + status to decide what to do.
function handleUpdate(msg, eventName) {
    if (!msg || typeof msg !== "object") return;

    // ASP.NET Core SignalR normally serializes Object/Status as object/status.
    // Accept either casing in case the server's JSON options change.
    const object = msg.object ?? msg.Object ?? eventName;
    if (typeof object !== "string" || !object.trim()) return;
    const status = msg.status ?? msg.Status ?? "";

    const sessionCode = GetSessionCode();
    const messageSession = msg.sessionCode ?? msg.SessionCode;
    // The server's {sessionCode}_Web group provides actual session isolation.
    // If the payload also carries a session code, reject a mismatch.
    if (messageSession && messageSession !== sessionCode) return;

    window.dispatchEvent(new CustomEvent("split-signal-update", {
        detail: { object, status: String(status), sessionCode }
    }));
}

// Automatic SignalR reconnect creates a new connection. Rejoin its group.
conn.onreconnected(async () => {
    const sessionCode = GetSessionCode();
    if (!sessionCode) return;
    try {
        await conn.invoke("JoinSession", sessionCode, "Web");
        console.log("SignalR group rejoined:", sessionCode);
        window.dispatchEvent(new Event("split-signal-reconnected"));
    } catch (error) {
        console.error("Could not rejoin SignalR group:", error);
    }
});

// ---------------------------------------
// SignalR Connection
// ---------------------------------------

async function startSignalR() {

    try {

        console.log("In startSignalR method");

        sessionCode = document.getElementById("SessionCode").value;
        demoMode = document.getElementById("DemoMode-CheckBox").checked ? "true" : "false";

        console.log(`Demo status: ${demoMode}.`);

        sessionStorage.setItem("demoMode", demoMode);

        console.log(`Using session code ${sessionCode}`);

        await conn.start();

        console.log(`After conn.start(). ${conn.state}`);

        result = true;

        await conn.invoke(
            "JoinSession",
            String(sessionCode).trim(),
            "Web"
        );

        console.log(`This is the result ${result}.`);

        if (result) {
            console.log(`Connected to SignalR with ${String(sessionCode).trim() }`);
            sessionStorage.setItem("sessionCode", String(sessionCode).trim());
            window.location.href = "/Game";
        }
        else {
            console.log("Session does not exist.");
        }

    }
    catch (err) {

        console.error(err);

    }
}

async function reconnectSession() {
    const sessionCode = sessionStorage.getItem("sessionCode");


    console.log(`Attempting reconnect with session ${sessionCode}`);

    if (!sessionCode) {
        window.location.replace("/");
        return;
    }

    try {
        if (conn.state === signalR.HubConnectionState.Disconnected) {
            await conn.start();
        }

        await conn.invoke(
            "JoinSession",
            sessionCode,
            "Web"
        );

        console.log("Rejoined session:", sessionCode);
    }
    catch (err) {
        console.error("Failed to reconnect:", err);
    }
}


// ---------------------------------------
// Update Request
// ---------------------------------------

async function UpdateReq(object, status, notify) {

    // The game page is a new document: always read the stored session code.
    const sessionCode = GetSessionCode();
    if (!sessionCode) {
        console.error("Update request failed: no session code in sessionStorage.");
        return false;
    }

    const req = {
        sessionCode: sessionCode,
        object: object,
        status: status,
        notify: notify
    };

    console.log(req);

    try {

        const response = await fetch(API_PATH + "Update", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(req)
        });

        if (!response.ok) {
            throw new Error(
                `HTTP error: ${response.status}`
            );
        }

        console.log("Update request complete!");
        return true;

    }
    catch (err) {

        console.error("Update request failed:", err);
        return false;

    }
}



// Keep compatibility with existing Razor onclick handlers and other scripts.
window.startSignalR = startSignalR;
window.reconnectSession = reconnectSession;
window.UpdateReq = UpdateReq;


// ---------------------------------------
// Check Request
// ---------------------------------------

async function CheckReq(objects) {

    const req = {
        sessionCode: GetSessionCode(),
        objects: objects
    };

    console.log(req);

    try {

        const response = await fetch(API_PATH + "Check", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(req)
        });

        if (!response.ok) {
            throw new Error(
                `HTTP error: ${response.status}`
            );
        }

        console.log("Check request complete!");

        // If Check returns JSON, retrieve it here.
        const result = await response.json();

        console.log(result);

        return result;

    }
    catch (err) {

        console.error("Check request failed:", err);

    }
}


// ---------------------------------------
// Session
// ---------------------------------------

function GetSessionCode() {
    const sessionCode = sessionStorage.getItem("sessionCode")
    return sessionCode;
}

window.CheckReq = CheckReq;
window.GetSessionCode = GetSessionCode;
