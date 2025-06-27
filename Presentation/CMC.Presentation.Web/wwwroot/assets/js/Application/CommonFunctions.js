var Settings = {
    OnLoad: function () {
        Settings.InitializeFileUpload();
        Settings.InitializeFontSizePreview();
        Settings.LoadCurrentFontSizes();
    },

    InitializeFileUpload: function () {
        $('#background-img').on('change', function (e) {
            var file = e.target.files[0];
            if (file) {
                $('#lblBackgroundImgName').text(file.name);
                $('#lblBackgroundImgStatus').removeClass('fa-upload').addClass('fa-check text-success');
            } else {
                $('#lblBackgroundImgName').text(globalResources.ChooseImg);
                $('#lblBackgroundImgStatus').removeClass('fa-check text-success').addClass('fa-upload');
            }
        });
    },

    InitializeFontSizePreview: function () {
        $('#SystemFontSize').on('change', function () {
            $('#system-preview').css('font-size', $(this).val());
        });

        $('#CompetitionFontSize').on('change', function () {
            $('#competition-preview').css('font-size', $(this).val());
        });
    },

    LoadCurrentFontSizes: function () {
        // Set preview based on current selected values
        $('#system-preview').css('font-size', $('#SystemFontSize').val());
        $('#competition-preview').css('font-size', $('#CompetitionFontSize').val());
    },

    Save: function () {
        // Show loading
        $("#loading").css("display", "flex");

        // Clear previous validation messages
        $('.field-validation-valid').html('').hide();

        // Prepare form data manually
        var formData = new FormData();

        // Add font size settings
        formData.append('SystemFontSize', $('#SystemFontSize').val());
        formData.append('CompetitionFontSize', $('#CompetitionFontSize').val());

        // Add background image if selected
        var fileInput = $('#background-img')[0];
        if (fileInput.files && fileInput.files[0]) {
            formData.append('BackgroundImg', fileInput.files[0]);
        }

        $.ajax({
            type: 'POST',
            url: publicURls.UpdateSettings,
            data: formData,
            contentType: false,
            processData: false,
            success: function (data) {
                $("#loading").hide();

                if (data.resultCode == 422) {
                    // Handle validation errors from server
                    for (var i = 0; i < data.brokenRoles.length; i++) {
                        var propertyName = data.brokenRoles[i]["propertyName"];
                        var message = data.brokenRoles[i]["message"];
                        $('.field-validation-valid').show();
                        var errorElement = $("span[data-valmsg-for='" + propertyName + "']");
                        errorElement.html(message);

                        // Check if the error element is visible at the top of the page
                        if (errorElement.is(":visible") && errorElement.offset().top < $(window).scrollTop()) {
                            // Scroll the page to the top to make the error message visible
                            $("html, body").animate({ scrollTop: errorElement.offset().top }, 500);
                        }
                    }
                }
                else if (data.resultCode == 200 || data.isSuccess) {
                    Swal.fire({
                        title: '',
                        text: data.msg || globalResources.SystemSettingsUpdated,
                        icon: 'success',
                        showConfirmButton: false,
                        timer: 2000
                    }).then(function (result) {
                        // Reload page to apply new font sizes
                        window.location.reload();
                    });
                }
                else {
                    GeneralClass.ShowErrorAlert(data.msg || globalResources.ErrorOccurred);
                }
            },
            error: function (xhr, status, error) {
                $("#loading").hide();

                if (xhr.status === 422) {
                    // Handle validation errors
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (response.brokenRoles) {
                            for (var i = 0; i < response.brokenRoles.length; i++) {
                                var propertyName = response.brokenRoles[i]["propertyName"];
                                var message = response.brokenRoles[i]["message"];
                                $('.field-validation-valid').show();
                                var errorElement = $("span[data-valmsg-for='" + propertyName + "']");
                                errorElement.html(message);
                            }
                        }
                    } catch (e) {
                        GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                    }
                } else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            }
        });
    }
};