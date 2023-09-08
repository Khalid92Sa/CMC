var Login = {
    OnLoad: function () {
        $('#Password').keypress(function (e) {
            if (e.keyCode == 13) {
                Login.SignIn();
            }
        });
    },
    SignIn: function () {
        $('.field-validation-valid').html('');
        $.ajax({
            url: publicURls.login,
            type: 'POST',
            data: $('#LoginForm').serialize(),
            dataType: 'json',
            success: function (data) {
                if (data.resultCode != 200) {
                    for (var i = 0; i < data.brokenRoles.length; i++) {
                        $("span[data-valmsg-for='" + data.brokenRoles[i]["propertyName"] + "']").html(data.brokenRoles[i]["message"]);
                    }
                    $('.field-validation-valid').show();
                    return false;
                }
                else {
                    window.location.href = publicURls.HomePage;
                }
            },
            error: function (er) {

            }
        });
    },
    SignOut: function () {
        window.location.href = publicURls.logout;
    }
}