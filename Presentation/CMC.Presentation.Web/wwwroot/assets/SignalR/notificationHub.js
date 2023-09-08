
$(function () {
    var hub = $.connection.notificationHub;

    hub.client.receivedAll = function (message) {
        notifyMessage(message);
    };

    hub.client.receivedUser = function (message) {
        notifyMessage(message);
    };

    hub.client.receivedGroup = function (message) {
        notifyMessage(message);
    };

    $.connection.hub.start();
});

function notifyMessage(message) {
    if (message.url !== '') {
        toastr.options.onclick = function () {
            window.location = message.url;
        };
    }

    toastr.info(message.sender + ' ' + message.content, "Notification");
}