
let socket;
const button = document.getElementById("ButtonConnectSocket");

const ConnOpen = () => {
    console.log("Connected to server");
    button.parentElement.style.borderColor = "green";
}

const ConnMsg = (msg) => {
    console.log("Message received:", msg.data);
    ParsePayload(msg.data);
}

const ConnClose = () => {
    console.log("Discontected from server");
    button.parentElement.style.borderColor = "red";
}

const CreateSocket = () => {
    socket = new WebSocket("ws://localhost:9000/");
    socket.onopen = ConnOpen;
    socket.onclose = ConnClose;
    socket.onmessage = ConnMsg;
}

const ParsePayload = (payload) => {
    // /204ms/ [90,94183731079102%] $PLD$ RoutedEventArgs e)
    const index_confidence = [
        payload.indexOf("["),
        payload.indexOf("]")
    ]

    const index_data = payload.search(/\$PLD\$/) + 6;
    console.log(index_data);

    const confidence = Number(payload.substring(index_confidence[0] + 1, index_confidence[1] - 1).replace(",", "."));
    const data = payload.substring(index_data, payload.length - 1);

    if(confidence < 75) return;

    const index_coords = [
        data.indexOf("("),
        data.indexOf(")")
    ];
    const raw_coords = data.substring(index_coords[0] + 1, index_coords[1]);
    let coords = raw_coords.split(",");
    console.log(coords);

    document.getElementById("coord").value = coords[0] + "," + coords[1];
    const x = String(coords[0]);
    const y = String(coords[2]);
    origin.setLatLng([x, y]);
}

button.addEventListener("click", () => {
    if(typeof socket === "undefined") {
        CreateSocket();
        console.log("Created socket!");
        return;
    }

    const state = socket.readyState;
    if(state === WebSocket.CONNECTING) {
        console.log("Socket is connecting...");
    } else if(state === WebSocket.OPEN) {
        console.log("Socket is already open");
    } else if(state === WebSocket.CLOSING) {
        console.log("Socket is closing");
    } else if(state === WebSocket.CLOSED) {
        CreateSocket();
        console.log("Created socket!");
    }
});
