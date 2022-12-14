"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/posHub").configureLogging(signalR.LogLevel.Debug).build();

//Disable the send button until connection is established.
document.getElementById("testButton").disabled = true;

connection.on("ReceiveTxn", function ( store, register, item, total) {
    var li = document.createElement("li");
    document.getElementById("txnList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    document.getElementById("storeId").innerText = `${store}`;
    li.textContent = `Store: ${store}, Register ${register}, Item ${item} : ${total}`;
});

connection.start().then(function () {
    document.getElementById("testButton").disabled = false; 
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("testButton").addEventListener("click", function (event) {
    var register = document.getElementById("registerInput").value;
    var item = document.getElementById("itemId").value;
    console.error("item id: " + item);
    var total = document.getElementById("totalInput").value;
    connection.invoke("SendTxn", register, item, Number(total)).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});