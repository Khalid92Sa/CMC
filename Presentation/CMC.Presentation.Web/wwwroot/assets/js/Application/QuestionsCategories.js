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


                //Render the image
                var reader = new FileReader();
                reader.onload = function (e) {
                    $('#dv-img-category').find('img').removeAttr('style');
                    $('#dv-img-category').find('img').attr('src', e.target.result); // Update the img src
                    $('#dv-btn-delete-image-category').removeAttr('style');
                }
                reader.readAsDataURL($(this).get(0).files[0]);


                var fileName = categoryAttachment.files[0].name;
                if (fileName.length > 30) {
                    fileName = fileName.substring(0, 30);
                }
                $('#lblImageName').text(fileName);
                $('#lblImgStatus').removeClass('fa-upload');
                $('#lblImgStatus').addClass('fa-check-circle');
            }
            else {
                Categories.ClearAttachment($('#file-category-img'), globalResources.ChooseImg);
                $("#Img-category-invalid").text("");
            }
        });

        GeneralClass.InitalizeDatePicker('txtDate', undefined);
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
    DeleteExistingImg: function (type, idImg, e) {
        var idOfImg = $('#hdn-curr-img-id').val();
        if (idOfImg == '' && type != 3) {
            //Means user delete the original image
            //means in catgeory or question, user delete the image for first time, and upload second image then delete again.
            if (type == 1) {
                //Category
                $('#file-category-img').val('');

                $('#lblImageName').text(globalResources.ChooseImg);
                $('#lblImgStatus').removeClass('fa-check-circle');
                $('#lblImgStatus').addClass('fa-upload');

                $('#dv-btn-delete-image-category').attr('style', 'display:none');
                $('#dv-img-category').find('img').attr('src', '');
                $('#dv-img-category').find('img').attr('style', 'display:none');
            }
            else if (type == 2) {
                //Question
                $('#question-img').val('');

                $('#lblQuestionImgName').text(globalResources.ChooseImg);
                $('#lblQuestionImgStatus').removeClass('fa-check-circle');
                $('#lblQuestionImgStatus').addClass('fa-upload');

                $('#dv-btn-delete-image-question').attr('style', 'display:none');
                $('#dv-img-question').find('img').attr('src', '');
                $('#dv-img-question').find('img').attr('style', 'display:none');
            }
            return;
        }
        else if (type == 3) {
            var currentAnswer = $(e).attr('data-id');
            var idExistAnswer = $('#hdn-option-img-id-' + currentAnswer).val();

            if (idExistAnswer == '') {
                // This is new answer, just delete the image that user uploaded.
                $(e).hide();
                $('#answer-img-' + currentAnswer).val(''); // clear file input

                $('#lblImageName-' + currentAnswer).text(globalResources.ChooseImg);
                $('#lblImgStatus-' + currentAnswer).removeClass('fa-check-circle');
                $('#lblImgStatus-' + currentAnswer).addClass('fa-upload');

                $('#dv-btn-delete-image-ans-' + currentAnswer).attr('style', 'display:none');
                $('#dv-imgOptionAns-' + currentAnswer).find('img').attr('src', '');
                $('#dv-imgOptionAns-' + currentAnswer).find('img').attr('style', 'display:none');
                return;
            }
            else {
                idOfImg = idExistAnswer;
            }
        }

        $.ajax({
            type: 'POST',
            url: publicURls.DeleteCurrentCategoryImg,
            data: {
                id: idOfImg,
                type: type
            },
            success: function (data) {
                if (data.isSuccess) {
                    $(e).hide(); // hide the button of delete
                    if (type != 3) {
                        //Clear current id for question and category.
                        $('#hdn-curr-img-id').val('');
                    }
                    else {
                        $('#hdn-option-img-id-' + currentAnswer).val(''); // clear the original Id
                    }

                    var statusDiv = '';
                    if (idImg == 'dv-img-category') {
                        statusDiv = 'dv-delete-category-status';
                        $('#file-category-img').val('');
                        $('#lblImageName').text(globalResources.ChooseImg);
                        $('#lblImgStatus').removeClass('fa-check-circle');
                        $('#lblImgStatus').addClass('fa-upload');
                        $('#dv-btn-delete-image-category').attr('style', 'display:none');
                    }
                    else if (idImg == 'dv-img-question') {
                        statusDiv = 'dv-delete-question-status';
                        $('#question-img').val('');
                        $('#lblQuestionImgName').text(globalResources.ChooseImg);
                        $('#lblQuestionImgStatus').removeClass('fa-check-circle');
                        $('#lblQuestionImgStatus').addClass('fa-upload');
                        $('#dv-btn-delete-image-question').attr('style', 'display:none');
                    }
                    else {
                        //Answer
                        statusDiv = 'dv-delete-answer-status-' + currentAnswer;
                        $('#answer-img-' + currentAnswer).val(''); // clear file input
                        $('#lblImageName-' + currentAnswer).text(globalResources.ChooseImg);
                        $('#lblImgStatus-' + currentAnswer).removeClass('fa-check-circle');
                        $('#lblImgStatus-' + currentAnswer).addClass('fa-upload');
                        $('#dv-btn-delete-image-ans-' + currentAnswer).attr('style', 'display:none');
                    }


                    $('#' + statusDiv).find('.i-success-delete-img').show();
                    $('#' + statusDiv).find('.i-error-delete-img').hide();
                    $('#' + idImg).find('img').attr('src', '');
                    $('#' + idImg).find('img').attr('style', 'display:none');
                }
                else {
                    $('#' + statusDiv).find('.i-error-delete-img').show();
                    $('#' + statusDiv).find('.i-success-delete-img').hide();
                }

                $('#' + statusDiv).find('.spn-delete-img-status').html(data.msg);
                $('#' + statusDiv).show();
                if (data.isSuccess) {
                    setTimeout(function () {
                        $('#' + statusDiv).hide();
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
        var QuestionsDate = $('#txtDate').val();
        var CategoryId = $('#Id').val();
        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Questions/GetAllQuestions?pageNumber=' + pageIndex + '&pageSize=' + pageSize + '&CategoryId=' + CategoryId + '&QuestionText=' + QuestionText + '&Date=' + QuestionsDate
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
    BulkImport: {
        importedQuestions: [],
        isInitialized: false, // Add this flag
        OnLoad: function () {
            console.log('BulkImport OnLoad called, isInitialized:', Categories.BulkImport.isInitialized);

            // Prevent multiple initializations
            if (Categories.BulkImport.isInitialized) {
                console.log('Already initialized, skipping...');
                return;
            }

            Categories.BulkImport.initializeUploadZone();
            Categories.BulkImport.initializeEventHandlers();
            Categories.BulkImport.isInitialized = true;

            console.log('BulkImport initialized successfully');
        },
        initializeUploadZone: function () {
            console.log('Initializing upload zone...');

            const uploadZone = document.getElementById('uploadZone');
            const fileInput = document.getElementById('excelFile');

            if (!uploadZone || !fileInput) {
                console.log('Upload zone or file input not found');
                return;
            }

            // Remove ALL event listeners (both jQuery and vanilla JS)
            $(uploadZone).off();
            $(fileInput).off();

            // Clone and replace the elements to remove ALL event listeners
            const newUploadZone = uploadZone.cloneNode(true);
            const newFileInput = fileInput.cloneNode(true);

            uploadZone.parentNode.replaceChild(newUploadZone, uploadZone);
            fileInput.parentNode.replaceChild(newFileInput, fileInput);

            // Add single event listener using vanilla JavaScript
            newUploadZone.addEventListener('click', function (e) {
                console.log('Upload zone clicked - vanilla JS');
                e.preventDefault();
                e.stopPropagation();
                newFileInput.click();
            }, { once: false }); // Allow multiple clicks

            // Add file change listener
            newFileInput.addEventListener('change', function (e) {
                console.log('File input changed - vanilla JS');
                if (this.files && this.files.length > 0) {
                    console.log('File selected:', this.files[0].name);
                    Categories.BulkImport.handleFileUpload(this.files[0]);
                }
            });

            console.log('Upload zone initialized with vanilla JS');
        },
        initializeEventHandlers: function () {
            // Select all questions
            $('#selectAllQuestions').on('click', function () {
                const checkboxes = $('.question-checkbox');
                const allChecked = checkboxes.toArray().every(cb => cb.checked);
                checkboxes.prop('checked', !allChecked);
                Categories.BulkImport.updateSelectedQuestions();
            });

            // Save selected questions
            $('#saveSelectedQuestions').on('click', function () {
                Categories.BulkImport.saveSelectedQuestions();
            });

            // Handle individual checkbox changes
            $(document).on('change', '.question-checkbox', function () {
                Categories.BulkImport.updateSelectedQuestions();
            });

            // Handle edit and remove buttons
            $(document).on('click', '.edit-question-btn', function () {
                const questionId = $(this).data('question-id');
                Categories.BulkImport.editQuestion(questionId);
            });

            $(document).on('click', '.remove-question-btn', function () {
                const questionId = $(this).data('question-id');
                Categories.BulkImport.removeQuestion(questionId);
            });
        },
        handleFileUpload: function (file) {
            if (!file.name.match(/\.(xlsx|xls)$/)) {
                GeneralClass.ShowErrorAlert(globalResources.InvalidExcelFile || 'Please select a valid Excel file');
                return;
            }

            // Show loading
            Categories.BulkImport.showLoading(true);

            // Create FormData for file upload
            const formData = new FormData();
            formData.append('excelFile', file);
            formData.append('categoryId', $('#ddlBulkCategories').val());

            // AJAX call to backend ProcessExcelFile method
            $.ajax({
                url: publicURls.ProcessExcelFile,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    Categories.BulkImport.showLoading(false);

                    if (response.isSuccess) {
                        // Filter out empty questions and store the questions data
                        Categories.BulkImport.importedQuestions = response.questions
                            .filter(q => {
                                // Filter out questions that are completely empty
                                const hasQuestionText = (q.textEn && q.textEn.trim()) || (q.textAr && q.textAr.trim());
                                const hasAnswers = q.answers && q.answers.length > 0;
                                const hasValidAnswers = q.answers && q.answers.some(a =>
                                    (a.textEn && a.textEn.trim()) || (a.textAr && a.textAr.trim())
                                );

                                return hasQuestionText && hasAnswers && hasValidAnswers;
                            })
                            .map((q, index) => ({
                                id: index + 1,
                                questionEn: q.textEn || '',
                                questionAr: q.textAr || '',
                                answers: (q.answers || []).filter(a =>
                                    // Filter out empty answer options
                                    (a.textEn && a.textEn.trim()) || (a.textAr && a.textAr.trim())
                                ),
                                categoryId: q.categoryId,
                                isValid: true,
                                warnings: [],
                                selected: true,
                                isEditing: false
                            }));

                        // Process and display questions
                        Categories.BulkImport.validateQuestions();
                        Categories.BulkImport.updateStatistics();
                        Categories.BulkImport.renderQuestions();
                        Categories.BulkImport.showQuestionsContainer();

                        // Show success message
                        Swal.fire({
                            title: globalResources.QuestionsAddedSuccessfully,
                            text: response.message,
                            icon: 'success',
                            timer: 3000,
                            showConfirmButton: false
                        });
                    } else {
                        GeneralClass.ShowErrorAlert(response.message);
                    }
                },
                error: function (xhr, status, error) {
                    Categories.BulkImport.showLoading(false);
                    console.error('Excel upload error:', error);
                    GeneralClass.ShowErrorAlert(globalResources.ExcelProcessingError || 'Error processing Excel file');
                }
            });
        },
        validateQuestions: function () {
            Categories.BulkImport.importedQuestions.forEach(question => {
                question.warnings = [];
                question.isValid = true;

                // Basic validation
                if (!question.questionEn && !question.questionAr) {
                    question.isValid = false;
                    question.warnings.push(globalResources.QuestionTextRequired || 'Question text is required');
                }

                if (!question.answers || question.answers.length < 2) {
                    question.isValid = false;
                    question.warnings.push(globalResources.MinimumTwoAnswers || 'At least 2 answer options are required');
                }

                // Check if at least one answer is marked as correct
                if (question.answers && !question.answers.some(a => a.isAnswer)) {
                    question.isValid = false;
                    question.warnings.push(globalResources.NoCorrectAnswer || 'No correct answer specified');
                }

                // Warnings for missing translations
                if (question.questionEn && !question.questionAr) {
                    question.warnings.push(globalResources.ArabicTranslationMissing || 'Arabic translation missing');
                }
                if (question.questionAr && !question.questionEn) {
                    question.warnings.push(globalResources.EnglishTranslationMissing || 'English translation missing');
                }
            });
        },
        updateStatistics: function () {
            const total = Categories.BulkImport.importedQuestions.length;
            const valid = Categories.BulkImport.importedQuestions.filter(q => q.isValid && q.warnings.length === 0).length;
            const withWarnings = Categories.BulkImport.importedQuestions.filter(q => q.isValid && q.warnings.length > 0).length;
            const invalid = Categories.BulkImport.importedQuestions.filter(q => !q.isValid).length;

            $('#totalQuestions').text(total);
            $('#validQuestions').text(valid);
            $('#warningQuestions').text(withWarnings);
            $('#invalidQuestions').text(invalid);
        },
        renderQuestions: function () {
            const container = $('#questionsListContainer');
            container.empty();

            Categories.BulkImport.importedQuestions.forEach((question, index) => {
                const questionCard = Categories.BulkImport.createQuestionCard(question, index);
                container.append(questionCard);
            });
        },
        createQuestionCard: function (question, index) {
            const statusClass = !question.isValid ? 'border-danger' :
                (question.warnings.length > 0 ? 'border-warning' : 'border-success');

            const statusIcon = question.isValid ?
                (question.warnings.length > 0 ? '<i class="fas fa-exclamation-triangle text-warning"></i>' :
                    '<i class="fas fa-check-circle text-success"></i>') :
                '<i class="fas fa-times-circle text-danger"></i>';

            const warningsHtml = question.warnings.length > 0 ? `
        <div class="mt-2">
            <small class="text-warning">
                <i class="fas fa-exclamation-triangle me-1"></i>
                ${question.warnings.join(', ')}
            </small>
        </div>
    ` : '';

            return $(`
        <div class="question-card-clean ${statusClass}" data-question-id="${question.id}">
            <div class="p-3">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <div class="d-flex align-items-center">
                        <input type="checkbox" class="form-check-input question-checkbox mr-2" 
                               ${question.selected ? 'checked' : ''} 
                               data-question-id="${question.id}"
                               ${!question.isValid ? 'disabled' : ''}>
                        <span class="badge badge-primary mr-2">${globalResources.Question} ${index + 1}</span>
                        ${statusIcon}
                    </div>
                    <div>
                        <button class="btn btn-sm btn-outline-primary mr-1 edit-question-btn" data-question-id="${question.id}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger remove-question-btn" data-question-id="${question.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                
                ${warningsHtml}
                
                <!-- Content Display Mode -->
                <div id="content-${question.id}" class="question-content">
                    <div class="row mt-3">
                        <div class="col-md-6">
                            <strong class="text-muted">${globalResources.English}:</strong>
                            <p class="mb-2">${question.questionEn || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}</p>
                        </div>
                        <div class="col-md-6">
                            <strong class="text-muted">${globalResources.Arabic}:</strong>
                            <p class="mb-2">${question.questionAr || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}</p>
                        </div>
                    </div>
                    
                    <div class="mt-3">
                        <strong class="text-muted mb-2 d-block">${globalResources.Options}:</strong>
                        ${Categories.BulkImport.createCleanAnswerOptions(question)}
                    </div>
                </div>

                <!-- Edit Mode (Hidden by default) -->
                <div id="edit-${question.id}" class="question-edit d-none">
                    ${Categories.BulkImport.createQuestionEditForm(question)}
                </div>
            </div>
        </div>
    `);
        },
        createCleanAnswerOptions: function (question) {
            if (!question.answers || question.answers.length === 0) {
                return `<p class="text-muted">${globalResources.NoAnswerOptions}</p>`;
            }

            const options = [];
            question.answers.forEach((answer, index) => {
                const isCorrect = answer.isAnswer;
                options.push(`
            <div class="answer-option-clean ${isCorrect ? 'correct' : ''}">
                <div class="row">
                    <div class="col-md-6">
                        <strong>${globalResources.Option} ${index + 1} (EN):</strong> 
                        ${answer.textEn || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}
                    </div>
                    <div class="col-md-6">
                        <strong>${globalResources.Option} ${index + 1} (AR):</strong> 
                        ${answer.textAr || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}
                    </div>
                </div>
            </div>
        `);
            });
            return options.join('');
        },
        createQuestionContent: function (question) {
            return `
        <div class="row">
            <div class="col-md-6">
                <h6 class="fw-bold">${globalResources.English}:</h6>
                <p class="mb-2">${question.questionEn || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}</p>
            </div>
            <div class="col-md-6">
                <h6 class="fw-bold">${globalResources.Arabic}:</h6>
                <p class="mb-2">${question.questionAr || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}</p>
            </div>
        </div>
        <div class="row mt-3">
            <div class="col-12">
                <h6 class="fw-bold mb-3">${globalResources.Options}:</h6>
                ${Categories.BulkImport.createAnswerOptions(question)}
            </div>
        </div>
    `;
        },
        createQuestionEditForm: function (question) {
            return `
        <div class="row">
            <div class="col-md-6">
                <div class="mb-3">
                    <label class="form-label fw-bold">${globalResources.QuestionEnglish}:</label>
                    <textarea class="form-control question-edit-en" rows="2">${question.questionEn}</textarea>
                </div>
            </div>
            <div class="col-md-6">
                <div class="mb-3">
                    <label class="form-label fw-bold">${globalResources.QuestionArabic}:</label>
                    <textarea class="form-control question-edit-ar" rows="2">${question.questionAr}</textarea>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-12">
                <h6 class="fw-bold mb-3">${globalResources.EditAnswerOptions}:</h6>
                ${Categories.BulkImport.createEditableAnswerOptions(question)}
            </div>
        </div>
        <div class="text-end mt-3">
            <button class="btn btn-success me-2 save-question-btn" data-question-id="${question.id}">
                <i class="fas fa-save me-1"></i>${globalResources.Save}
            </button>
            <button class="btn btn-secondary cancel-edit-btn" data-question-id="${question.id}">
                <i class="fas fa-times me-1"></i>${globalResources.Cancel}
            </button>
        </div>
        `;
        },
        createAnswerOptions: function (question) {
            if (!question.answers || question.answers.length === 0) {
                return `<p class="text-muted">${globalResources.NoAnswerOptions}</p>`;
            }

            const options = [];
            question.answers.forEach((answer, index) => {
                const isCorrect = answer.isAnswer;
                options.push(`
            <div class="answer-option ${isCorrect ? 'correct' : ''}">
                <div class="row">
                    <div class="col-md-6">
                        <strong>${globalResources.Option} ${index + 1} (EN):</strong> 
                        ${answer.textEn || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}
                    </div>
                    <div class="col-md-6">
                        <strong>${globalResources.Option} ${index + 1} (AR):</strong> 
                        ${answer.textAr || '<em class="text-muted">' + globalResources.NotProvided + '</em>'}
                    </div>
                </div>
            </div>
        `);
            });
            return options.join('');
        },
        createEditableAnswerOptions: function (question) {
            const options = [];
            const maxOptions = Math.max(4, question.answers ? question.answers.length : 0);

            for (let i = 0; i < maxOptions; i++) {
                const answer = question.answers && question.answers[i] ? question.answers[i] : { textEn: '', textAr: '', isAnswer: false };
                const isCorrect = answer.isAnswer;

                options.push(`
            <div class="col-md-6 mb-3">
                <div class="answer-edit-container">
                    <div class="d-flex align-items-center mb-2">
                        <input type="radio" name="correct-answer-${question.id}" value="${i + 1}" 
                               class="form-check-input me-2" ${isCorrect ? 'checked' : ''}>
                        <label class="form-label fw-bold mb-0">${globalResources.Option} ${i + 1}</label>
                    </div>
                    <input type="text" class="form-control mb-2 option-edit-en" 
                           placeholder="${globalResources.InEnglish}" 
                           value="${answer.textEn || ''}" data-option="${i + 1}">
                    <input type="text" class="form-control option-edit-ar" 
                           placeholder="${globalResources.InArabic}" 
                           value="${answer.textAr || ''}" data-option="${i + 1}">
                </div>
            </div>
        `);
            }
            return options.join('');
        },
        editQuestion: function (questionId) {
            const question = Categories.BulkImport.importedQuestions.find(q => q.id === questionId);
            if (!question) return;

            question.isEditing = true;

            // Hide content and show edit form
            $(`#content-${questionId}`).addClass('d-none');
            $(`#edit-${questionId}`).removeClass('d-none');

            // Add event handlers for the edit form buttons
            $(`#edit-${questionId}`).find('.save-question-btn').off('click').on('click', function () {
                Categories.BulkImport.saveQuestionEdit(questionId);
            });

            $(`#edit-${questionId}`).find('.cancel-edit-btn').off('click').on('click', function () {
                Categories.BulkImport.cancelQuestionEdit(questionId);
            });
        },
        saveQuestionEdit: function (questionId) {
            const question = Categories.BulkImport.importedQuestions.find(q => q.id === questionId);
            if (!question) return;

            const editContainer = $(`#edit-${questionId}`);

            // Update question text
            question.questionEn = editContainer.find('.question-edit-en').val();
            question.questionAr = editContainer.find('.question-edit-ar').val();

            // Update options
            question.answers = [];
            for (let i = 1; i <= 4; i++) {
                const optionEn = editContainer.find(`.option-edit-en[data-option="${i}"]`).val();
                const optionAr = editContainer.find(`.option-edit-ar[data-option="${i}"]`).val();

                if (optionEn || optionAr) {
                    question.answers.push({
                        textEn: optionEn,
                        textAr: optionAr,
                        isAnswer: false
                    });
                }
            }

            // Update correct answer
            const correctAnswer = parseInt(editContainer.find(`input[name="correct-answer-${questionId}"]:checked`).val()) || 1;
            if (question.answers[correctAnswer - 1]) {
                question.answers[correctAnswer - 1].isAnswer = true;
            }

            question.isEditing = false;

            // Re-validate and re-render
            Categories.BulkImport.validateQuestions();
            Categories.BulkImport.updateStatistics();

            // Update the specific question card
            const questionCard = $(`.question-card-clean[data-question-id="${questionId}"]`);
            const index = Categories.BulkImport.importedQuestions.findIndex(q => q.id === questionId);
            const newCard = Categories.BulkImport.createQuestionCard(question, index);
            questionCard.replaceWith(newCard);
        },
        cancelQuestionEdit: function (questionId) {
            const question = Categories.BulkImport.importedQuestions.find(q => q.id === questionId);
            if (!question) return;

            question.isEditing = false;

            // Show content and hide edit form
            $(`#content-${questionId}`).removeClass('d-none');
            $(`#edit-${questionId}`).addClass('d-none');
        },
        removeQuestion: function (questionId) {
            Swal.fire({
                title: globalResources.Alert_DeleteQuestion,
                text: globalResources.Alert_DeleteQuestion_Text || 'Are you sure you want to remove this question?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: globalResources.Delete,
                cancelButtonText: globalResources.Cancel
            }).then((result) => {
                if (result.isConfirmed) {
                    Categories.BulkImport.importedQuestions = Categories.BulkImport.importedQuestions.filter(q => q.id !== questionId);
                    Categories.BulkImport.updateStatistics();
                    Categories.BulkImport.renderQuestions();

                    Swal.fire({
                        title: '',
                        text: globalResources.QuestionRemovedSuccessfully,
                        icon: 'success',
                        showConfirmButton: false,
                        timer: 2000
                    });
                }
            });
        },
        saveSelectedQuestions: function () {
            const selectedCategoryId = $('#ddlBulkCategories').val();
            if (!selectedCategoryId || selectedCategoryId === '' || selectedCategoryId === null) {
                Swal.fire({
                    title: globalResources.ValidationErrors,
                    text: globalResources.CategorySelectionRequired,
                    icon: 'warning',
                    confirmButtonText: globalResources.Ok 
                });
                return;
            }

            const selectedQuestions = Categories.BulkImport.importedQuestions.filter(q => q.selected && q.isValid);

            if (selectedQuestions.length === 0) {
                GeneralClass.ShowErrorAlert(globalResources.NoValidQuestionsSelected);
                return;
            }

            // Show loading
            GeneralClass.showLoading();
            // Create the payload with exact property names matching C# model
            const questionsData = {
                Questions: selectedQuestions.map(q => ({
                    CategoryId: parseInt(q.categoryId || $('#ddlBulkCategories').val()) || null,
                    TextEn: q.questionEn || '',
                    TextAr: q.questionAr || '',
                    AnswertType: 1,
                    Answers: (q.answers || []).map(answer => ({
                        TextEn: answer.textEn || '',
                        TextAr: answer.textAr || '',
                        IsAnswer: Boolean(answer.isAnswer)
                    }))
                })),
                DefaultCategoryId: $('#ddlBulkCategories').val() ? parseInt($('#ddlBulkCategories').val()) : null
            };
            // Log the data being sent
            console.log('Sending data to server:', JSON.stringify(questionsData, null, 2));

            // Make the AJAX request
            $.ajax({
                type: 'POST',
                url: publicURls.AddBulkQuestions,
                data: JSON.stringify(questionsData),
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                processData: false,
                beforeSend: function (xhr) {
                    console.log('Request headers:', xhr.getAllResponseHeaders());
                },
                success: function (response) {
                    console.log('Server response:', response);
                    GeneralClass.hideLoading();

                    if (response.isSuccess) {
                        Swal.fire({
                            title: '',
                            text: `${globalResources.QuestionsImportedSuccessfully}` + ` ${selectedQuestions.length} ` + ` ${globalResources.TheQuestion2}`,
                            icon: 'success',
                            showConfirmButton: false,
                            timer: 3000
                        }).then(() => {
                            if ($('#ddlBulkCategories').val()) {
                                window.location.href = publicURls.AddCategory + '/' + $('#ddlBulkCategories').val();
                            } else {
                                window.location.href = publicURls.Categories;
                            }
                        });
                    } else {
                        if (response.resultCode === 422 && response.brokenRoles) {
                            let errorMessages = response.brokenRoles.map(rule => rule.message).join('<br>');
                            Swal.fire({
                                title: globalResources.ValidationErrors,
                                html: errorMessages,
                                icon: 'error'
                            });
                        } else {
                            GeneralClass.ShowErrorAlert(response.msg || globalResources.ErrorOccurred);
                        }
                    }
                },
                error: function (xhr, status, error) {
                    GeneralClass.hideLoading();
                    console.error('AJAX Error Details:');
                    console.error('Status:', status);
                    console.error('Error:', error);
                    console.error('Response Text:', xhr.responseText);
                    console.error('Status Code:', xhr.status);

                    GeneralClass.ShowErrorAlert('Request failed: ' + (xhr.responseText || error));
                }
            });
        },
        showQuestionsContainer: function () {
            $('#statsRow').removeClass('d-none');
            $('#questionsContainer').removeClass('d-none');
        },
        showLoading: function (show) {
            const spinner = $('.loading-spinner');
            if (show) {
                spinner.show();
            } else {
                spinner.hide();
            }
        },
        updateSelectedQuestions: function () {
            $('.question-checkbox').each(function () {
                const questionId = parseInt($(this).data('question-id'));
                const question = Categories.BulkImport.importedQuestions.find(q => q.id === questionId);
                if (question) {
                    question.selected = $(this).is(':checked');
                }
            });
        },
        CloseBulkModal: function () {
            $('#excelTemplateModal').modal('hide');
        }
    }
}

var AllCategories = {
    OnLoad: function () {
        $(document).ready(function () {
            AllCategories.GetAllWithPager(1, GeneralClass.pageSize);
        });
    },
    GetAllWithPager: function (pageIndex, pageSize) {
        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Questions/GetLastQuestions', null, null, this.getAll_Success, null);
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
            { data: "categoryName", sortable: false, name: "Category name", autoWidth: true }
        ];
        Grid.fillGrid('#tblLastQuestion', data.data, columns, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'Categories');
    }
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
            if ($(this).attr('id') == 'question-img') {
                //Question
                $('#question-Img-invalid').text('');
                if ($(this).get(0).files.length > 0) {
                    var currentFile = $(this).get(0).files;
                    var ext = currentFile[0].name;
                    var extt = ext.split('.').pop().toLowerCase();
                    if (extt != "png" && extt != "jpg" && extt != "jpeg") {
                        $("#question-Img-invalid").text(globalResources.InvalidAttachmentImg);
                        $("#question-Img-invalid").css("display", "block");
                        Questions.ClearAnswerAttachment($(this), globalResources.ChooseImg, '', true);
                        return false;
                    }

                    //Render the image
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        //$('#' + btnToHide).hide(); // btnToHide = id of button to be hidden
                        $('#dv-img-question').find('img').removeAttr('style');
                        $('#dv-img-question').find('img').attr('src', e.target.result); // Update the img src
                        $('#dv-btn-delete-image-question').removeAttr('style');
                    }
                    reader.readAsDataURL($(this).get(0).files[0]);

                    var fileName = currentFile[0].name;
                    if (fileName.length > 30) {
                        fileName = fileName.substring(0, 30);
                    }
                    $('#lblQuestionImgName').text(fileName);
                    $('#lblQuestionImgStatus').removeClass('fa-upload');
                    $('#lblQuestionImgStatus').addClass('fa-check-circle');
                }
            }
            else {
                //Answer
                var currentAnswerNum = $(this).attr('data-answer');
                $("#answer-Img-invalid-" + currentAnswerNum).text("");
                if ($(this).get(0).files.length > 0) {
                    var currentFile = $(this).get(0).files;
                    var ext = currentFile[0].name;
                    var extt = ext.split('.').pop().toLowerCase();
                    if (extt != "png" && extt != "jpg" && extt != "jpeg") {
                        $("#answer-Img-invalid-" + currentAnswerNum).text(globalResources.InvalidAttachmentImg);
                        $("#answer-Img-invalid-" + currentAnswerNum).css("display", "block");
                        Questions.ClearAnswerAttachment($(this), globalResources.ChooseImg, currentAnswerNum, false); // false = answer ... true = question
                        return false;
                    }

                    //Render the image
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $('#dv-imgOptionAns-' + currentAnswerNum).find('img').removeAttr('style');
                        $('#dv-imgOptionAns-' + currentAnswerNum).find('img').attr('src', e.target.result); // Update the img src
                        $('#dv-btn-delete-image-ans-' + currentAnswerNum).removeAttr('style');
                    }
                    reader.readAsDataURL($(this).get(0).files[0]);

                    var fileName = currentFile[0].name;
                    if (fileName.length > 30) {
                        fileName = fileName.substring(0, 30);
                    }

                    $('#lblImageName-' + currentAnswerNum).text(fileName);
                    $('#lblImgStatus-' + currentAnswerNum).removeClass('fa-upload');
                    $('#lblImgStatus-' + currentAnswerNum).addClass('fa-check-circle');
                }
            }
        });

        $('a[data-toggle="tab"], button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
            const target = $(e.target).attr('data-bs-target') || $(e.target).attr('href');

            if (target === '#bulk-import' && !Categories.BulkImport.isInitialized) {
                console.log('Bulk import tab shown, initializing...');
                Categories.BulkImport.OnLoad();
            }
        });

        $('#bulk-tab').one('click', function () {
            console.log('Bulk tab clicked for first time');
            setTimeout(function () {
                Categories.BulkImport.OnLoad();
            }, 300);
        });

        // For subsequent clicks, just check if it's active
        $('#bulk-tab').on('click', function () {
            if ($(this).hasClass('active') && !Categories.BulkImport.isInitialized) {
                setTimeout(function () {
                    Categories.BulkImport.OnLoad();
                }, 300);
            }
        });

        $('#single-tab').on('click', function () {
            console.log('Single tab clicked'); // Debug log
            // Any single tab initialization if needed
        });

        // Synchronize category selection between tabs
        $('#ddlCategories').on('change', function () {
            $('#ddlBulkCategories').val($(this).val());
        });

        $('#ddlBulkCategories').on('change', function () {
            $('#ddlCategories').val($(this).val());
        });

        //Categories.BulkImport.OnLoad();
    },
    ArchiveNonArchive: function (type) {
            Swal.fire({
            title: globalResources.Alert_ArchiveQuestions,
            text: '',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: globalResources.Yes,
            cancelButtonText: globalResources.Cancel,
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    type: 'GET',
                    url: publicURls.ArchiveQuestions,
                    data: {
                        type: type, // 1 = Archive, 2 = Unarchive
                        categoryId: $('#Id').val()
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
                                    window.location.reload();
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
        });
    },
    SelectAnswerByImage: function (id) {
        $('#chk-option-img-' + id).prop('checked', 'checked');
    },

    ClearAnswerAttachment(fileInput, defaultMessage, num, isQuestion) {
        $(fileInput).val('');
        if (isQuestion) {
            //Question
            if (defaultMessage != '') {
                $('#lblQuestionImgName').text(defaultMessage);
            }
            $('#lblQuestionImgStatus').removeClass('fa-check-circle');
            $('#lblQuestionImgStatus').addClass('fa-upload');
        }

        else {
            //Answer
            if (defaultMessage != '') {
                $('#lblImageName-' + num).text(defaultMessage);
            }
            $('#lblImgStatus-' + num).removeClass('fa-check-circle');
            $('#lblImgStatus-' + num).addClass('fa-upload');
        }
    },
    AddNewQuestion: function () {

        if ($('#bulk-import').hasClass('show') && $('#bulk-import').hasClass('active')) {
            Categories.BulkImport.saveSelectedQuestions();
            return;
        }


        $('.field-validation-valid').html('').hide();
        var data = new FormData();
        data.append("Id", $('#Id').val());
        data.append("CategoryId", $('#ddlCategories').val());
        data.append("TextEn", $("#TextEn").val().trim());
        data.append("TextAr", $("#TextAr").val().trim());

        var questionImg = $('#question-img');
        data.append("Img", $(questionImg).get(0).files[0]);
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
                            $(option1Ar).parent().find('.field-validation-valid').html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option1Ar).parent().find('.field-validation-valid').html('').hide();
                        }

                        //Option 2 Arabic
                        if (option2Ar.val().trim() == '') {
                            $(option2Ar).parent().find('.field-validation-valid').html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option2Ar).parent().find('.field-validation-valid').html('').hide();
                        }
                    }
                    else {
                        //Option 1 English
                        if (option1En.val().trim() == '') {
                            $(option1En).parent().find('.field-validation-valid').html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option1En).parent().find('.field-validation-valid').html('').hide();
                        }

                        //Option 2 English
                        if (option2En.val().trim() == '') {
                            $(option2En).parent().find('.field-validation-valid').html(globalResources.OptionRequired).show();
                            isValid = false;
                        }
                        else {
                            $(option2En).parent().find('.field-validation-valid').html('').hide();
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
                    if (option3En.val() != '' || option3Ar.val() != '') {
                        data.append('Answers[' + 2 + '].TextEn', option3En.val().trim());
                        data.append('Answers[' + 2 + '].TextAr', option3Ar.val().trim());
                        data.append('Answers[' + 2 + '].IsAnswer', checkOption3);
                        data.append('Answers[' + 2 + '].Id', option3Id.val());
                    }


                    //Add Option 4 to Data
                    if (option4En.val() != '' || option4Ar.val() != '') {
                        data.append('Answers[' + 3 + '].TextEn', option4En.val().trim());
                        data.append('Answers[' + 3 + '].TextAr', option4Ar.val().trim());
                        data.append('Answers[' + 3 + '].IsAnswer', checkOption4);
                        data.append('Answers[' + 3 + '].Id', option4Id.val());
                    }

                }
                else {

                    // Images Answeres
                    //In Case create new question
                    if ($('#Id').val() == '') {
                        if ($('#answer-img-1').val() == '' || $('#answer-img-2').val() == '') {
                            $('#lbl-generic-error').html(globalResources.PleaseAddTwoImagesAtLeast).show();
                            return false;
                        }
                        else {
                            $('#lbl-generic-error').html('').hide();
                        }
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
                    if ($(option3Img).val() != '' || option3ImgId.val() != '') {
                        data.append('Answers[' + 2 + '].Img', $(option3Img).get(0).files[0]);
                        data.append('Answers[' + 2 + '].IsAnswer', checkOptionImg3);
                        data.append('Answers[' + 2 + '].Id', option3ImgId.val());
                    }


                    //Add Option 4 to Data
                    if ($(option4Img).val() != '' || option4ImgId.val() != '') {
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
    },
    AddBulkQuestions: function (questionsData) {
        $.ajax({
            type: 'POST',
            url: publicURls.AddBulkQuestions,
            data: JSON.stringify(questionsData),
            contentType: 'application/json',
            processData: false,
            success: function (response) {
                if (response.isSuccess) {
                    Swal.fire({
                        title: '',
                        text: globalResources.QuestionsAddedSuccessfully || 'Questions added successfully!',
                        icon: 'success',
                        showConfirmButton: false,
                        timer: 2000
                    }).then(() => {
                        window.location.href = publicURls.Categories;
                    });
                } else {
                    if (response.resultCode === 422 && response.brokenRoles) {
                        for (var i = 0; i < response.brokenRoles.length; i++) {
                            var propertyName = response.brokenRoles[i]["propertyName"];
                            var message = response.brokenRoles[i]["message"];
                            console.error(`Validation error for ${propertyName}: ${message}`);
                        }
                    }
                    GeneralClass.ShowErrorAlert(response.message || globalResources.ErrorOccurred);
                }
            },
            error: function (xhr, status, error) {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    }
}