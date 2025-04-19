// app.js
const socket = new WebSocket('wss://localhost:7159'); 

const resultsList = document.getElementById('results');

// Evento cuando se abre la conexión
socket.addEventListener('open', () => {
    console.log('WebSocket connection established.');
});
socket.addEventListener('message', (event) => {
    const message = event.data; // Obtener el mensaje enviado por el servidor
    console.log('Message received:', message);

    // Crear un nuevo elemento de lista para mostrar el mensaje
    const listItem = document.createElement('li');
    listItem.textContent = message;
    resultsList.appendChild(listItem); // Agregar el mensaje a la lista
});