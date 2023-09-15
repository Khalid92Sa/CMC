var AlertsType = {
    Info: 'info',
    Warning: 'warning',
    Danger: 'danger',
    Error: 'error',
    Success: 'success'
};

var GeneralClass = {
    pageSize: generalSettings.pageSize,
    showLoading: function () {
        //$('#wait_overlay').show();
        $("#loading").css("display", "flex");
    },
    hideLoading: function () {
        setTimeout(function () {
            $("#loading").css("display", "none");
        }, 100);
    },
    ToggleLanguage: function (lang) {
        var returnedUrl = $('#hdnReturnedUrl').val();
        $.ajax({
            url: publicURls.setCulture,
            type: 'post',
            data: { 'culture': lang, 'returnUrl': returnedUrl },
            success: function (data) {
                location.reload();
            },
            error: function (er) {
                consol.log(er);
            }
        });
    },
    validationAlphaName: function (el, evt, lang) {
        lang = typeof lang !== 'undefined' ? lang : '';

        var charCode = (evt.which) ? evt.which : evt.keyCode;

        var charKey = (evt.char) ? evt.char : evt.key;

        var validate;

        if (typeof evt.key == 'undefined')
            validate = true;

        if (charCode === 32 || (charCode >= 48 && charCode <= 57)) // Allow space and numbers
            validate = true;
        if (lang == '') {

            if (charCode == 1567 || charCode == 1548) // ؟ ،
                validate = false;

            // Update the regular expression to allow numbers and exclude Arabic characters
            var letter = /^[-_\'’a-zA-Z0-9 ?-]+$/i;

            validate = letter.test(charKey);
        }
        else if (lang == 'en') {

            // Update the regular expression to allow numbers and exclude Arabic characters
            var en = /^[-_\'a-z0-9 ?-]+$/i;

            validate = en.test(charKey);

            if (!validate) {
                if ($(el).next().hasClass('field-validation-valid')) {
                    $(el).next().html(globalResources.EnglishFieldValidation);
                    $('.field-validation-valid').show();
                }
            }
        }
        else if (lang == 'ar') {

            if (charCode == 1567 || charCode == 1548) // ؟ ،
                validate = false;

            //var ar = /^[-\_\’\ \u0600-\u06FF]+$/i;
            var ar = /^[-_\'\’0-9\u0600-\u06FF ?-]+$/i;

            validate = ar.test(charKey);

            if (!validate) {
                if ($(el).next().hasClass('field-validation-valid')) {
                    $(el).next().html(globalResources.ArabicFieldValidation);
                    $('.field-validation-valid').show();
                }
                return false; // Return false if validation fails
            }
        }

        if (validate) {
            if ($(el).next().hasClass('field-validation-valid'))
                $(el).next().html('');
        }
        return validate;
    },
    validationNumbersOnly: function (el, e) {
        if (e.shiftKey || e.charCode == 46)
            return false;

        if (e.charCode >= 48 && e.charCode <= 57)
            return true;

        if (e.which == 8)
            return true;

        if ((e.ctrlKey === true || e.metaKey === true) || (e.charCode == 8) ||

            (e.keyCode >= 35 && e.keyCode <= 39)) {

            return true;
        }

        if (((e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }

        return false;
    },
    validateFloatKeyPress: function (el, evt) {
        var elValue = el.value;

        var charCode = (evt.which) ? evt.which : event.keyCode;
        var number = elValue.split('.');
        if (charCode != 44 && charCode != 46 && charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }

        if (number.length > 1 && charCode == 46) {
            return false;
        }

        var caratPos = el.selectionStart;

        if (charCode == 46 && (elValue.length - caratPos) > 2) {
            return false;
        }

        var dotPos = elValue.indexOf(".");

        if (caratPos > dotPos && dotPos > -1 && (number[1].length > 1)) {
            return false;
        }

        if (charCode != 46)
            if ((dotPos == -1 && (number[0].replace(/,/g, '').length > 9))
                || (dotPos > -1 && caratPos <= dotPos && (number[0].replace(/,/g, '').length > 9)))
                return false;

        return true;
    },
    MoneyFormat: function (id) {
        if (typeof id !== 'undefined') {
            var val = $('#' + id).val().split(',').length <= 1 ? new Intl.NumberFormat().format(parseInt($('#' + id).val())) : $('#' + id).val();
            $('#' + id).val(val);
        }
        else {
            $('.price').each(function () {
                var val = $(this).html().replace(/[^\d.-]/g, ''); // Remove non-numeric characters
                val = parseFloat(val); // Parse the number as a float
                if (isNaN(val)) {
                    // Handle the case where the value is not a number
                    val = $(this).html();
                } else {
                    // Format the number using the user's locale
                    val = val.toLocaleString();
                }
                $(this).html(val);
            });
        }
    },
    MoneyFormatText: function () {
        var moneyTextBoxes = $('.money-format');

        // Apply money format to existing values on document load
        moneyTextBoxes.each(function () {
            var value = $(this).val().replace(/,/g, ''); // Remove existing commas from the value
            var formattedValue = formatMoney(value); // Format the value with commas
            $(this).val(formattedValue); // Update the textbox value with the formatted value
        });

        moneyTextBoxes.on('input', function () {
            var value = $(this).val().replace(/,/g, ''); // Remove existing commas from the value
            var formattedValue = formatMoney(value); // Format the value with commas

            // Temporarily disable validation for the current textbox
            $(this).rules('remove');

            // Update the textbox value with commas
            $(this).val(formattedValue);

            // Re-enable validation
            $(this).rules('add');

            // Trigger validation manually
            $(this).valid();
        });
    },
    IsMobile: function () {
        var width = $(window).width();
        if (width >= 1024) {
            return false;
        }
        else {
            return true;
        }
    },
    ActivationMenuItem: function (menu, item) {
        $('a').removeClass('active');
        $(menu).addClass('active').siblings().removeClass('active');
        $(item).addClass('active').siblings().removeClass('active');
        $(menu).click();
    },
    ActivationSubMenuItem: function (menu, subMenu, item) {
        $(menu).addClass('active').siblings().removeClass('active');
        $(menu).click();
        $(subMenu).addClass('active').siblings().removeClass('active');
        $(subMenu).click();
        $(item).addClass('active').siblings().removeClass('active');
    },
    SweetAlert: function (msg, title, type, url) {
        Swal.fire({
            title: title,
            text: msg,
            icon: type,
            showConfirmButton: false,
            timer: 3000
        }).then((result) => {
            if (url != '') {
                window.location = url;
            }
        });
    },
    ShowErrorAlert: function (message) {
        var $alert = $("#alert-error");
        if ($alert.data("active")) { return; }
        $alert.empty();
        if (message != '' && message != undefined) {
            $alert.append(message);
        }
        else {
            $alert.append(globalResources.ErrorOccurred);
        }
        $alert.show("slow").data("active", true);
        setTimeout(function () {
            $alert.hide("slow").data("active", false);
        }, 5000);
    },
    InitalizeDatePicker: function (txtDateId,minDate) {
        $(function () {
            var calendarG = $.calendars.umElQuraInstance("Gregorian", "ar-EG");
            var options = {
                calendar: calendarG,
                dateFormat: 'yyyy/mm/dd',
                monthsToShow: [1, 1],
                showOtherMonths: true
            };
            if (minDate !== undefined) {
                options.minDate = minDate;
            }


            //$('#' + txtDateId).calendarsPicker({
            //    calendar: calendarG,
            //    dateFormat: 'yyyy/mm/dd',
            //    //yearRange: '1912:2029',
            //    monthsToShow: [1, 1], showOtherMonths: true
            //});
            $('#' + txtDateId).calendarsPicker(options);
            $('#' + txtDateId).keypress(function () { return false; });
        });
    }

}


$(document).ajaxStart(function () {
    var showLoader = sessionStorage.getItem("showLoader");
    if (showLoader != undefined && showLoader != '' && showLoader == 'false') {
        GeneralClass.hideLoading();
    }
    else {
        GeneralClass.showLoading();
    }
});
$(document).ajaxStop(function () {
    GeneralClass.hideLoading();
});

function formatMoney(value) {
    var parts = value.toString().split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    return parts[0];
}