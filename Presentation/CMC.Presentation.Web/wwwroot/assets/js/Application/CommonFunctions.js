var Settings = {
    OnLoad: function () {
        $('.custom-file-input').change(function (e) {
            //Question
            $('#background-Img-invalid').text('');
            if ($(this).get(0).files.length > 0) {
                var currentFile = $(this).get(0).files;
                var ext = currentFile[0].name;
                var extt = ext.split('.').pop().toLowerCase();
                if (extt != "png" && extt != "jpg" && extt != "jpeg") {
                    $("#background-Img-invalid").text(globalResources.InvalidAttachmentImg);
                    $("#background-Img-invalid").css("display", "block");
                    $(this).val('');
                    $('#lblBackgroundImgName').text(globalResources.ChooseImg);
                    $('#dv-img-background').addClass('d-none');
                    return false;
                }

                //Render the image
                var reader = new FileReader();
                reader.onload = function (e) {
                    $('#dv-img-background').find('img').removeAttr('style');
                    $('#dv-img-background').find('img').attr('src', e.target.result); // Update the img src
                    $('#dv-img-background').removeClass('d-none');
                }
                reader.readAsDataURL($(this).get(0).files[0]);

                var fileName = currentFile[0].name;
                if (fileName.length > 30) {
                    fileName = fileName.substring(0, 30);
                }
                $('#lblBackgroundImgName').text(fileName);
            }
        });
    },
    Save: function () {
        var data = new FormData();
        var backgroundImg = $('#background-img');
        data.append("BackgroundImg", $(backgroundImg).get(0).files[0]);

        $.ajax({
            type: "POST",
            url: publicURls.UpdateSettings,
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                if (!data.isSuccess) {
                    if (data.resultCode == 422) {
                        for (var i = 0; i < data.brokenRoles.length; i++) {
                            var propertyName = data.brokenRoles[i]["propertyName"];
                            var message = data.brokenRoles[i]["message"];
                            var errorElement = $("span[data-valmsg-for='" + propertyName + "']");
                            if (errorElement == undefined || errorElement.length == 0) {
                                errorElement = $('#lbl-generic-error');
                            }
                            errorElement.html(message).show();
                            // Check if the error element is visible at the top of the page
                            if (errorElement.is(":visible") && errorElement.offset().top < $(window).scrollTop()) {
                                // Scroll the page to the top to make the error message visible
                                $("html, body").animate({ scrollTop: errorElement.offset().top }, 500);
                            }
                        }
                    }
                    else {
                        GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                    }
                }
                else {
                    GeneralClass.SweetAlert(data.msg, '', 'success', publicURls.HomePage);
                }
            },
            error: function () {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                GeneralClass.hideLoading();
            }
        });
    }
}