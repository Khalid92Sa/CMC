document.addEventListener('DOMContentLoaded', function () {
    // When the event DOMContentLoaded occurs, it is safe to access the DOM
    $(window).scroll(function () {
        var sticky = $('#navbar'),
            scroll = $(window).scrollTop();

        if (scroll >= 100) sticky.addClass('sticky');
        else sticky.removeClass('sticky');


    });

});
//(function () {
//    const idleDurationSecs = 900;    // X number of seconds
//    const redirectUrl = "javascript:document.getElementById('logoutForm').submit()";  // Redirect idle users to this URL
//    let idleTimeout; // variable to hold the timeout, do not modify

//    const resetIdleTimeout = function () {
//        // Clears the existing timeout
//        if (idleTimeout) {
//            clearTimeout(idleTimeout);
//        }

//        // Set a new idle timeout to load the redirectUrl after idleDurationSecs
//        idleTimeout = setTimeout(() => {
//            window.location.replace(redirectUrl);
//        }, idleDurationSecs * 1000);
//    };

//    // Init on page load
//    resetIdleTimeout();

//    // Reset the idle timeout on any of the events listed below
//    ['click', 'touchstart', 'mousemove'].forEach(evt =>
//        document.addEventListener(evt, resetIdleTimeout, false)
//    );

//    window.addEventListener("storage", function (e) {
//        if (e.key === "idleTimeout" && e.newValue > e.oldValue) {
//            resetIdleTimeout();
//        }
//    });
//    ['click', 'touchstart', 'mousemove'].forEach(evt =>
//        document.addEventListener(evt, function () {
//            localStorage.setItem("idleTimeout", Date.now());
//        }, false)
//    );
//})();
var width = $(window).width();

$(document).ready(function () {
    var mobile = $("#mobile").html();
    var web = $("#web").html();
    // alert(mobile);
    if (width >= 1024) {
        $("#mobile").empty();
        $("#web").empty();
        $("#web").append(web);
    }
    else {
        $("#web").empty();
        $("#mobile").empty();
        $("#mobile").append(mobile);
    }

    //$(window).resize(function () {
    //    var width1 = $(window).width();
    //    if (width1 >= 1024) {
    //        $("#mobile").empty();
    //        $("#web").empty();
    //        $("#web").append(web);
    //    }
    //    else {
    //        $("#web").empty();
    //        $("#mobile").empty();
    //        $("#mobile").append(mobile);
    //    }
    //});

});



//if (width >= 1024) {
//    $("#mobile").remove();
//}
//else {
//    $("#web").remove();
//}