var Categories = {
    OnLoad: function () {
        $('#file-category-img').change(function () {
            var categoryAttachment = document.getElementById("file-category-img");
            $("#Img-category-invalid").text("");
            if (categoryAttachment.files.length > 0) {
                var ext = categoryAttachment.files[0].name;
                var extt = ext.split('.').pop().toLowerCase();
                if (extt != "png" && extt != "jpg" && extt != "jpeg") {
                    $("#Img-category-invalid").text(globalResources.InvalidAttachmentImg);
                    $("#Img-category-invalid").css("display", "block");
                    Categories.ClearAttachment($('#file-category-img'), globalResources.ChooseImg);
                    return false;
                }

                $('#lblImageName').text(categoryAttachment.files[0].name);
                $('#lblImgStatus').removeClass('fa-upload');
                $('#lblImgStatus').addClass('fa-check-circle');
            }
            else {
                Categories.ClearAttachment($('#file-category-img'), globalResources.ChooseImg);
                $("#Img-category-invalid").text("");
            }
        });

        $(document).ready(function () {
            Categories.GetAllWithPager(1, GeneralClass.pageSize);
            $('#btnSearch').on('click', function () {
                $("#pagination").twbsPagination('destroy');
                Categories.GetAllWithPager(1, GeneralClass.pageSize);
            });
        });
    },
    ClearAttachment: function (fileInput, defaultMessage) {
        $(fileInput).val('');
        $('#lblImageName').text(defaultMessage);
        $('#lblImgStatus').removeClass('fa-check-circle');
        $('#lblImgStatus').addClass('fa-upload');
    },
    CreateOrUpdate: function () {
        $('.field-validation-valid').html('').hide();
        var data = new FormData();
        data.append("Id", $("#Id").val());
        data.append("NameEn", $("#NameEn").val());
        data.append("NameAr", $("#NameAr").val());
        const fileInput = document.getElementById('file-category-img');
        if (fileInput.files.length > 0) {
            data.append("Img", fileInput.files[0]);
        }

        $.ajax({
            type: 'POST',
            url: $('#form-category').attr('action'),
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.resultCode == 422) {
                    for (var i = 0; i < data.brokenRoles.length; i++) {
                        var propertyName = data.brokenRoles[i]["propertyName"];
                        var message = data.brokenRoles[i]["message"];
                        var errorElement = $("span[data-valmsg-for='" + propertyName + "']");
                        errorElement.html(message);
                        // Check if the error element is visible at the top of the page
                        if (errorElement.is(":visible") && errorElement.offset().top < $(window).scrollTop()) {
                            // Scroll the page to the top to make the error message visible
                            $("html, body").animate({ scrollTop: errorElement.offset().top }, 500);
                        }
                    }
                    $('.field-validation-valid').show();
                }
                else if (data.resultCode == 200) {
                    Swal.fire({
                        title: '',
                        text: data.msg,
                        icon: 'success',
                        showConfirmButton: false,
                        timer: 2000
                    }).then(function (result) {
                        window.location = publicURls.Categories;
                    });
                }
                else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                GeneralClass.ShowErrorAlert();
            }
        });
    },
    DeleteCategory: function () {
        if ($('#num-questions').val() != '' && parseInt($('#num-questions').val()) != 0) {
            // In case there is question for category
            Swal.fire({
                title: globalResources.Alert_DeleteCategory,
                text: globalResources.Alert_DeleteCategory_Text,
                icon: 'warning',
                showDenyButton: true,
                showCancelButton: true,
                confirmButtonText: globalResources.Alert_Btn_DeleteCategoryWithQuestion,
                denyButtonText: globalResources.Alert_Btn_DeleteOnlyCategory,
                cancelButtonText: globalResources.Cancel,
            }).then((result) => {
                if (result.isConfirmed) {
                    //With Questions
                    $.ajax({
                        type: 'POST',
                        url: publicURls.DeleteCategory,
                        data: {
                            id: $('#Id').val(),
                            withQuestions: true
                        },
                        success: function (data) {
                            if (data.isSuccess) {
                                Swal.fire({
                                    title: '',
                                    text: data.msg,
                                    icon: 'success',
                                    showConfirmButton: false,
                                    timer: 2000
                                }).then(function (result) {
                                    window.location = publicURls.Categories;
                                });
                            }
                            else {
                                GeneralClass.ShowErrorAlert();
                            }
                        },
                        error: function (error) {
                            GeneralClass.ShowErrorAlert();
                        }
                    });

                } else if (result.isDenied) {
                    //Without
                    $.ajax({
                        type: 'POST',
                        url: publicURls.DeleteCategory,
                        data: {
                            id: $('#Id').val(),
                            withQuestions: false
                        },
                        success: function (data) {
                            if (data.isSuccess) {
                                Swal.fire({
                                    title: '',
                                    text: data.msg,
                                    icon: 'success',
                                    showConfirmButton: false,
                                    timer: 2000
                                }).then(function (result) {
                                    window.location = publicURls.Categories;
                                });
                            }
                            else {
                                GeneralClass.ShowErrorAlert();
                            }
                        },
                        error: function (error) {
                            GeneralClass.ShowErrorAlert();
                        }
                    });
                }
            })
        }
        else {
            // There is no question for category
            Swal.fire({
                title: globalResources.Alert_DeleteCategory,
                text: '',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: globalResources.Delete,
                cancelButtonText: globalResources.Cancel,
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        type: 'POST',
                        url: publicURls.DeleteCategory,
                        data: {
                            id: $('#Id').val(),
                            withQuestions: true
                        },
                        success: function (data) {
                            if (data.isSuccess) {
                                Swal.fire({
                                    title: '',
                                    text: data.msg,
                                    icon: 'success',
                                    showConfirmButton: false,
                                    timer: 2000
                                }).then(function (result) {
                                    window.location = publicURls.Categories;
                                });
                            }
                            else {
                                GeneralClass.ShowErrorAlert();
                            }
                        },
                        error: function (error) {
                            GeneralClass.ShowErrorAlert();
                        }
                    });
                }
            })
        }
    },
    DeleteExistingImg: function () {
        $.ajax({
            type: 'POST',
            url: publicURls.DeleteCurrentCategoryImg,
            data: {
                id: $('#Id').val()
            },
            success: function (data) {
                if (data.isSuccess) {
                    $('#i-success-delete-img-category').show();
                    $('#i-error-delete-img-category').hide();
                    $('#btnDeleteExistingImg').hide();
                }
                else {
                    $('#i-error-delete-img-category').show();
                    $('#i-success-delete-img-category').hide();
                }
                $('#spn-delete-category-img-status').html(data.msg);
                $('#dv-delete-category-status').show();

                if (data.isSuccess) {
                    setTimeout(function () {
                        $('#dv-delete-category-status').hide();
                    }, 2000);
                }
            },
            error: function (error) {
                GeneralClass.ShowErrorAlert();
            }
        });
    },
    GetAllWithPager: function (pageIndex, pageSize) {
        var QuestionText = $('#txtQuestionText').val();
        var CategoryId = $('#Id').val();
        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Questions/GetAllQuestions?pageNumber=' + pageIndex + '&pageSize=' + pageSize + '&CategoryId=' + CategoryId + '&QuestionText=' + QuestionText
            , null, null, this.getAll_Success, null);
    },
    getAll_Success: function (data, textStatus, xhr) {
        if (data != undefined) {
            currentPage = data.currentPage;
        }
        var page = data.currentPage;
        if (page === 0) { page = 1; }
        var columns = [
            {
                data: '', sortable: false,
                render: function (data, type, row, meta) {
                    return (meta.row + meta.settings._iDisplayStart + 1) + ((currentPage - 1) * GeneralClass.pageSize);
                }
            },
            { data: "text", sortable: false, name: "Question text", autoWidth: true },
            { data: "time", sortable: false, name: "Question time", autoWidth: true },
            { data: "points", sortable: false, name: "Question points", autoWidth: true },
            {
                "data": "Id",
                "name": "Id",
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var div = document.createElement('div');
                    if (CanCreate) {
                        var btnUpdate = document.createElement('a');
                        $(btnUpdate).attr('id', 'btnUpdate_' + row.id);
                        $(btnUpdate).attr('title', globalResources.Edit);
                        $(btnUpdate).attr('onclick', 'Questions.EditQuestion(this)');
                        $(btnUpdate).addClass('mx-1 action bg-accent');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-pen');
                        $(btnUpdate).append(pen);
                        div.append(btnUpdate);
                    }

                    if (CanDelete) {
                        var btndelete = document.createElement('a');
                        $(btndelete).attr('id', 'btndelete_' + row.id);
                        $(btndelete).attr('title', globalResources.Delete);
                        $(btndelete).attr('onclick', 'Questions.DeleteQuestion(this)');
                        $(btndelete).addClass('mx-1 action bg-danger');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-trash');
                        $(btndelete).append(pen);
                        div.append(btndelete);
                    }
                    return $(div).html();
                }
            }
        ];
        Grid.fillGrid('#tblQuestions', data.data, columns, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'Categories');
    },

}

var Questions = {
    OnLoad: function () {

        $('#ddlAnswersType').on('change', function () {
            if ($(this).val() == 2) {
                $('.dv-answers-text').addClass('d-none');
                $('.dv-answers-images').removeClass('d-none');
            }
            else {
                $('.dv-answers-text').removeClass('d-none');
                $('.dv-answers-images').addClass('d-none');
            }
        });

        $('.custom-file-input').change(function (e) {
            var currentAnswerNum = $(this).attr('data-answer');
            $("#answer-Img-invalid-" + currentAnswerNum).text("");
            if ($(this).get(0).files.length > 0) {
                var currentFile = $(this).get(0).files;
                var ext = currentFile[0].name;
                var extt = ext.split('.').pop().toLowerCase();
                if (extt != "png" && extt != "jpg" && extt != "jpeg") {
                    $("#answer-Img-invalid-" + currentAnswerNum).text(globalResources.InvalidAttachmentImg);
                    $("#answer-Img-invalid-" + currentAnswerNum).css("display", "block");
                    Questions.ClearAnswerAttachment($(this), globalResources.ChooseImg, currentAnswerNum);
                    return false;
                }

                $('#lblImageName-' + currentAnswerNum).text(currentFile[0].name);
                $('#lblImgStatus-' + currentAnswerNum).removeClass('fa-upload');
                $('#lblImgStatus-' + currentAnswerNum).addClass('fa-check-circle');
            }
        });
    },
    ClearAnswerAttachment(fileInput, defaultMessage,num) {
        $(fileInput).val('');
        $('#lblImageName-'+num).text(defaultMessage);
        $('#lblImgStatus-' + num).removeClass('fa-check-circle');
        $('#lblImgStatus-' + num).addClass('fa-upload');
    },
    AddNewQuestion: function () {
        $('.field-validation-valid').html('').hide();
        var data = new FormData();
        data.append("Id", $('#Id').val());
        data.append("CategoryId", $('#ddlCategories').val());
        data.append("TextEn", $("#TextEn").val().trim());
        data.append("TextAr", $("#TextAr").val().trim());
        data.append("Time", $("#Time").val().trim());
        data.append("Points", $("#Points").val().trim());
        data.append("AnswertType", $("#ddlAnswersType").val());

        //Answers
        isvalid = true;
        if ($("#ddlCategories").val() != '' && $("#Time").val() != '' && $("#Points").val() != '') {
            if ((IsArabic && $("#TextAr").val() != '') || (!IsArabic && $("#TextEn").val() != '')) {

                if ($('#ddlAnswersType').val() == 1) {
                    // Text answers
                    //Check which mandatory textbox to check based on Language
                    if (IsArabic) {
                        if ($('#txt-option-ar-1').val() == '' || $('#txt-option-ar-2').val() == '') {
                            $('#lbl-generic-error').html(globalResources.PleaseAddTwoOptions).show();
                            return false;
                        }
                        else {
                            $('#lbl-generic-error').html('').hide();
                        }
                    }
                    else {
                        if ($('#txt-option-en-1').val() == '' || $('#txt-option-en-2').val() == '') {
                            $('#lbl-generic-error').html(globalResources.PleaseAddTwoOptions).show();
                            return false;
                        }
                        else {
                            $('#lbl-generic-error').html('').hide();
                        }
                    }


                    // Option 1
                    var option1Id = $('#hdn-option-id-1');
                    var option1Ar = $('#txt-option-ar-1');
                    var option1En = $('#txt-option-en-1');
                    var checkOption1 = $('#chk-option-1').is(':checked');

                    // Option 2
                    var option2Id = $('#hdn-option-id-2');
                    var option2Ar = $('#txt-option-ar-2');
                    var option2En = $('#txt-option-en-2');
                    var checkOption2 = $('#chk-option-2').is(':checked');

                    // Option 3
                    var option3Id = $('#hdn-option-id-3');
                    var option3Ar = $('#txt-option-ar-3');
                    var option3En = $('#txt-option-en-3');
                    var checkOption3 = $('#chk-option-3').is(':checked');

                    // Option 4
                    var option4Id = $('#hdn-option-id-4');
                    var option4Ar = $('#txt-option-ar-4');
                    var option4En = $('#txt-option-en-4');
                    var checkOption4 = $('#chk-option-4').is(':checked');


                    var isValid = true;
                    //Validate Options
                    if (IsArabic) {
                        //Option 1 Arabic
                        if (option1Ar.val().trim() == '') {
                            $(option1Ar).parent().next().html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option1Ar).parent().next().html('').hide();
                        }

                        //Option 2 Arabic
                        if (option2Ar.val().trim() == '') {
                            $(option2Ar).parent().next().html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option2Ar).parent().next().html('').hide();
                        }
                    }
                    else {
                        //Option 1 English
                        if (option1En.val().trim() == '') {
                            $(option1En).parent().next().html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option1En).parent().next().html('').hide();
                        }

                        //Option 2 English
                        if (option2En.val().trim() == '') {
                            $(option2En).parent().next().html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option2En).parent().next().html('').hide();
                        }
                    }

                    if (!isValid) {
                        return false;
                    }



                    //Check if answer is selected or not
                    if (!checkOption1 && !checkOption2 && !checkOption3 && !checkOption4) {
                        $('#lbl-generic-error').html(globalResources.PleaseSelectCorrectAnswerBeforeContinue).show();
                        return false;
                    }
                    else {
                        $('#lbl-generic-error').html('').hide();
                    }


                    //Add Option 1 to Data
                    data.append('Answers[' + 0 + '].TextEn', option1En.val().trim());
                    data.append('Answers[' + 0 + '].TextAr', option1Ar.val().trim());
                    data.append('Answers[' + 0 + '].IsAnswer', checkOption1);
                    data.append('Answers[' + 0 + '].Id', option1Id.val());

                    //Add Option 2 to Data
                    data.append('Answers[' + 1 + '].TextEn', option2En.val().trim());
                    data.append('Answers[' + 1 + '].TextAr', option2Ar.val().trim());
                    data.append('Answers[' + 1 + '].IsAnswer', checkOption2);
                    data.append('Answers[' + 1 + '].Id', option2Id.val());


                    //Add Option 3 to Data
                    data.append('Answers[' + 2 + '].TextEn', option3En.val().trim());
                    data.append('Answers[' + 2 + '].TextAr', option3Ar.val().trim());
                    data.append('Answers[' + 2 + '].IsAnswer', checkOption3);
                    data.append('Answers[' + 2 + '].Id', option3Id.val());


                    //Add Option 4 to Data
                    data.append('Answers[' + 3 + '].TextEn', option4En.val().trim());
                    data.append('Answers[' + 3 + '].TextAr', option4Ar.val().trim());
                    data.append('Answers[' + 3 + '].IsAnswer', checkOption4);
                    data.append('Answers[' + 3 + '].Id', option4Id.val());
                }
                else {
                    // Images Answeres
                    if ($('#answer-img-1').val() == '' || $('#answer-img-2').val() == '') {
                        $('#lbl-generic-error').html(globalResources.PleaseAddTwoImagesAtLeast).show();
                        return false;
                    }
                    else {
                        $('#lbl-generic-error').html('').hide();
                    }


                    // Option 1
                    var option1ImgId = $('#hdn-option-img-id-1');
                    var option1Img = $('#answer-img-1');
                    var checkOptionImg1 = $('#chk-option-img-1').is(':checked');

                    // Option 2
                    var option2ImgId = $('#hdn-option-img-id-2');
                    var option2Img = $('#answer-img-2');
                    var checkOptionImg2 = $('#chk-option-img-2').is(':checked');

                    // Option 3
                    var option3ImgId = $('#hdn-option-img-id-3');
                    var option3Img = $('#answer-img-3');
                    var checkOptionImg3 = $('#chk-option-img-3').is(':checked');

                    // Option 4
                    var option4ImgId = $('#hdn-option-img-id-4');
                    var option4Img = $('#answer-img-4');
                    var checkOptionImg4 = $('#chk-option-img-4').is(':checked');


                    if (!checkOptionImg1 && !checkOptionImg2 && !checkOptionImg3 && !checkOptionImg4) {
                        $('#lbl-generic-error').html(globalResources.PleaseSelectCorrectAnswerBeforeContinue).show();
                        return false;
                    }
                    else {
                        $('#lbl-generic-error').html('').hide();
                    }

                    var option1ImgName = $('#answer-img-1').val().split('\\').pop(); // Get the file name
                    var option2ImgName = $('#answer-img-2').val().split('\\').pop();
                    var option3ImgName = $('#answer-img-3').val().split('\\').pop();
                    var option4ImgName = $('#answer-img-4').val().split('\\').pop();

                    var existingFileNames = [option1ImgName, option2ImgName, option3ImgName, option4ImgName];

                    var isOption1Unique = Questions.CheckDuplicatedFiles(option1ImgName, existingFileNames);
                    var isOption2Unique = Questions.CheckDuplicatedFiles(option2ImgName, existingFileNames);
                    var isOption3Unique = Questions.CheckDuplicatedFiles(option3ImgName, existingFileNames);
                    var isOption4Unique = Questions.CheckDuplicatedFiles(option4ImgName, existingFileNames);
                    if (isOption1Unique || isOption2Unique || isOption3Unique || isOption4Unique) {
                        $('#lbl-generic-error').html(globalResources.Message_PleaseCheckDuplicatedAnsweresImage).show();
                        return false;
                    }
                    else {
                        $('#lbl-generic-error').html('').hide();
                    }

                    //Add Option 1 to Data
                    data.append('Answers[' + 0 + '].Img', $(option1Img).get(0).files[0]);
                    data.append('Answers[' + 0 + '].IsAnswer', checkOptionImg1);
                    data.append('Answers[' + 0 + '].Id', option1ImgId.val());

                    //Add Option 2 to Data
                    data.append('Answers[' + 1 + '].Img', $(option2Img).get(0).files[0]);
                    data.append('Answers[' + 1 + '].IsAnswer', checkOptionImg2);
                    data.append('Answers[' + 1 + '].Id', option2ImgId.val());


                    //Add Option 3 to Data
                    if ($(option3Img).val() != '') {
                        data.append('Answers[' + 2 + '].Img', $(option3Img).get(0).files[0]);
                        data.append('Answers[' + 2 + '].IsAnswer', checkOptionImg3);
                        data.append('Answers[' + 2 + '].Id', option3ImgId.val());
                    }
                    

                    //Add Option 4 to Data
                    if ($(option4Img).val() != '') {
                        data.append('Answers[' + 3 + '].Img', $(option4Img).get(0).files[0]);
                        data.append('Answers[' + 3 + '].IsAnswer', checkOptionImg4);
                        data.append('Answers[' + 3 + '].Id', option4ImgId.val());
                    }
                }
            }
        }


        $.ajax({
            type: "POST",
            url: publicURls.AddNewQuestion,
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
                    GeneralClass.SweetAlert(data.msg, '', 'success', publicURls.AddCategory + '/' + data.category);
                }
            },
            error: function () {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                GeneralClass.hideLoading();
            }
        });
    },
    CheckDuplicatedFiles: function (name, array) {
        if (name == '') {
            return false;
        }
        else {
            return !(array.indexOf(name) === array.lastIndexOf(name));
        }
    },
    SelectAnswer: function (e) {

        if ($('#ddlAnswersType').val() == 2) {
            return;
        }
        //clear all green color on other textbox
        var nameCurrentRadio = $(e).attr('name');
        var radioOptions = $('input[name=' + nameCurrentRadio + ']');
        radioOptions.each(function (index, optionRadio) {
            if ($(optionRadio).is(':checked')) {
                $(optionRadio).attr('style', 'background-color:green;border-color:green');
                $(optionRadio).parent().next().attr('style', 'border-color:green');
                $(optionRadio).parent().next().next().attr('style', 'border-color:green');
            }
            else {
                $(optionRadio).attr('style', 'background-color:white');
                $(optionRadio).parent().next().removeAttr('style');
                $(optionRadio).parent().next().next().removeAttr('style');
            }
        });
    },
    EditQuestion: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.AddNewQuestion + '/?id=' + id;
    },
    DeleteQuestion: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        var categoryId = $(e).attr('data-category');
        Swal.fire({
            title: globalResources.Alert_DeleteQuestion,
            text: '',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: globalResources.Delete,
            cancelButtonText: globalResources.Cancel,
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: 'DELETE',
                    url: publicURls.DeleteQuestion,
                    data: {
                        id: id
                    },
                    success: function (data) {
                        if (data.isSuccess) {
                            Swal.fire({
                                title: '',
                                text: data.msg,
                                icon: 'success',
                                showConfirmButton: false,
                                timer: 2000
                            }).then(function (result) {
                                if ($("#pagination").length > 0) {
                                    //Delete from Pagination
                                    $("#pagination").twbsPagination('destroy');
                                    Categories.GetAllWithPager(1, GeneralClass.pageSize);
                                }
                                else {
                                    //Delete from question page.
                                    window.location.href = publicURls.AddCategory + '/' + categoryId;
                                }
                            });
                        }
                        else {
                            GeneralClass.ShowErrorAlert();
                        }
                    },
                    error: function (error) {
                        GeneralClass.ShowErrorAlert();
                    }
                });
            }
        })
    }
}