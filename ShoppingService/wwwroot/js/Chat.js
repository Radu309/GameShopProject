"use strict";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

const sendButton = document.getElementById("sendButton");
const senderId = document.getElementById("senderId");
const receiverId = document.getElementById("receiverId");
const messageToSend = document.getElementById("messageToSend");
const messagesList = document.getElementById("messagesList");
const usersList = document.getElementById("userList");

sendButton.disabled = true;

// Start the connection
connection.start()
    .then(() => {
        sendButton.disabled = false;
    })
    .catch(err => {
        console.error("Connection error:", err.toString());
    }); 

const encodeMessage = (user, message) => {
    const safeMessage = message
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
    return `${user} says ${safeMessage}`;
};

const appendMessageToList = (user, message) => {
    const encodedMsg = encodeMessage(user, message);
    const li = document.createElement("li");
    li.textContent = encodedMsg;
    messagesList.appendChild(li);
};

const updateUsersList = (user, message) => {
    const existingUsers = new Set(
        Array.from(usersList.children).map(
            (li) => li.querySelector(".user-name")?.textContent
        )
    );
    // Adaugă sau actualizează utilizatorii din lista primită
    usersList.forEach(({ name, lastMessage }) => {
        let userElement = Array.from(usersList.children).find(
            (li) => li.querySelector(".user-name")?.textContent === name
        );

        if (!userElement) {
            // Dacă utilizatorul nu există, îl adăugăm
            userElement = document.createElement("li");
            userElement.classList.add("user");
            userElement.innerHTML = `
                <span class="user-name">${name}</span>
                <span class="last-message">${lastMessage || ""}</span>
            `;
            usersList.appendChild(userElement);
        } else {
            // Actualizează mesajul utilizatorului existent
            const lastMessageElement = userElement.querySelector(".last-message");
            lastMessageElement.textContent = lastMessage || "";
        }

        usersList.prepend(userElement);
        existingUsers.delete(name);
    });

    // Elimină utilizatorii care nu mai există în lista primită
    existingUsers.forEach((name) => {
        const userElement = Array.from(usersList.children).find(
            (li) => li.querySelector(".user-name")?.textContent === name
        );
        if (userElement) {
            usersList.removeChild(userElement);
        }
    });
};

connection.on("ReceiveMessage", (user, message) => {
    console.log("Received broadcast message:", { user, message });
    appendMessageToList(user, message);
    updateUsersList(user, message);
});

// Handle send button click
sendButton.addEventListener("click", event => {
    event.preventDefault();

    const sender = senderId.value;
    const receiver = receiverId.value;
    const message = messageToSend.value;

    console.log("Message details:", { sender, receiver, message });

    if (receiver === "") {
        connection.invoke("SendMessageToAll", sender, message)
            .catch(err => {
                console.error("Error sending broadcast message:", err.toString());
            });
    } else {
        connection.invoke("SendMessageToOne", sender, receiver, message)
            .catch(err => {
                console.error("Error sending private message:", err.toString());
            });
    }
});
