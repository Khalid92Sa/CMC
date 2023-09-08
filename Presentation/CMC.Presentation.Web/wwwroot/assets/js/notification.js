$(function () {
    var hub = $.connection.notificationHub;
    hub.client.receivedAll = function (message) {
        getNotificationCount();
        notifyMessage(message);
    };
    hub.client.receivedUser = function (message) {
        getNotificationCount();
        notifyMessage(message);
    };
    hub.client.receivedGroup = function (message) {
        console.log("received group");
        getNotificationCount();
        notifyMessage(message);
    };
    $.connection.hub.start();
});
function notifyMessage(message) {
    if (message.url !== '') {
        var newUrl = message.url;
        if (message.url.indexOf("{0}") > -1)
            newUrl = message.url.replace('{0}', userLang);
        toastr.options.onclick = function () {
            window.location = newUrl;
        };
    }
    var body = userLang === "ar" ? message.contentAr : message.contentEn;
    var title = userLang === "ar" ? message.titleAr : message.titleEn;
    toastr.info(body, title);
}
function getNotificationCount() {
    $.ajax({
        url: root + 'NotificationsManagement/GetPushNotificationCount',
        type: 'GET',
        dataType: 'json',
        cache: false,
        async: false,
        global: false,
        success: function (result) {
            if (result.data) {
                $('#notificationCountSpan').html(result.count);
                $('#notificationsBodyItems').html('');
                $.each(result.messages, function (index, element) {
                    var u = element.URL === null || element.URL === "" ? "" : element.URL;
                    var url = root + "NotificationsManagement/PushNotificationView?nid=" + element.Id;
                    var elm = "<a class=\"dropdown-item py-2\" href=\"#\" onclick=\"javascript: gotoLocation(event, '" + url + "', '" + element.Id + "')\"><span class=\"alert-date small d-block\">" + element.CreatedOn + "</span> <span class=\"notificatin-text\">" + element.MessageAr + "</span></a>";
                    $('#notificationsBodyItems').append(elm);
                });
            }
        }
    });
}