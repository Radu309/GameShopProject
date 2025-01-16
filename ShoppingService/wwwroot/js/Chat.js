    const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub") 
    .build();

    connection.on("ReceiveMessage", (senderId, message) => {
    const msg = document.createElement("div");
    msg.textContent = `De la ${senderId}: ${message}`;
    document.getElementById("messages").appendChild(msg);
});

    // Eveniment: Înregistrare confirmată
    connection.on("Registered", (message) => {
    console.log(message);
});

    // Eveniment: Eroare
    connection.on("Error", (error) => {
    console.error(error);
});

    // Pornește conexiunea
    connection.start()
    .then(() => {
    const userId = prompt("Introdu ID-ul tău:");
    connection.invoke("RegisterUser", userId); // Înregistrare pe server
})
    .catch(err => console.error(err.toString()));

    // Trimiterea mesajelor către alt utilizator
    document.getElementById("sendButton").addEventListener("click", () => {
    const receiverId = document.getElementById("receiverInput").value;
    const message = document.getElementById("messageInput").value;

    connection.invoke("SendMessageToUser", receiverId, message)
    .catch(err => console.error(err.toString()));
});
