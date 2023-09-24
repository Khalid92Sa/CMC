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
                    if (data.resultCode == 422) {
                        for (var i = 0; i < data.brokenRoles.length; i++) {
                            $("span[data-valmsg-for='" + data.brokenRoles[i]["propertyName"] + "']").html(data.brokenRoles[i]["message"]);
                        }
                        $('.field-validation-valid').show();
                    }
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



var Users = {
    OnLoad: function () {
        $(document).ready(function () {
            Users.GetAllWithPager(1, GeneralClass.pageSize);
            $('#btnSearch').on('click', function () {
                $("#pagination").twbsPagination('destroy');
                Users.GetAllWithPager(1, GeneralClass.pageSize);
            });
        });
    },
    GetAllWithPager: function (pageIndex, pageSize) {
        var userName = $('#txtName').val();
        var phoneNumber = $('#txtPhoneNumber').val();
        var group = $('#ddlGroups').val();
        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Users/GetAllUsers?pageNumber=' + pageIndex +
            '&pageSize=' + pageSize +
            '&Name=' + userName +
            '&PhoneNumber=' + phoneNumber +
            '&GroupId=' + group
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
            { data: "name", sortable: false, name: "Username", autoWidth: true },
            { data: "phoneNumber", sortable: false, name: "Phone Number", autoWidth: true },
            { data: "emailAddress", sortable: false, name: "Email Address", autoWidth: true },
            { data: "groupName", sortable: false, name: "Group ", autoWidth: true },
            {
                "data": "IsActive",
                "autoWidth": true,
                "render": function (IsActive, type, row) {
                    var span = document.createElement('span');
                    $(span).addClass('switch switch-sm');
                    var checkbox = document.createElement('input');
                    $(checkbox).attr('type', 'checkbox');
                    $(checkbox).attr('id', 'act-user-' + row.id);
                    $(checkbox).addClass('switch');
                    $(checkbox).attr('data-value', row.id);
                    $(checkbox).attr('data-status', IsActive);
                    $(checkbox).attr('onchange', 'Users.ActivationUsersConfirm(this)');
                    if (row.isActive) {
                        $(checkbox).attr('checked', 'checked');
                    }
                    var label = document.createElement('label');
                    $(label).addClass('mb-0');
                    $(label).attr('for', 'act-user-'+row.id);
                    $(span).append(checkbox);
                    $(span).append(label);
                    var div = document.createElement('div');
                    div.append(span);

                    var labelActive = document.createElement('label');
                    $(labelActive).addClass('pl-3');
                    var textNode = row.isActive ? globalResources.Active : globalResources.InActive;
                    var textLabel = document.createTextNode(textNode);
                    $(labelActive).append(textLabel);
                    div.append(labelActive);

                    return $(div).html();
                }
            },
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
                        $(btnUpdate).attr('onclick', 'Users.EditUser(this)');
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
                        $(btndelete).attr('onclick', 'Users.DeleteUser(this)');
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
        Grid.fillGrid('#tblUsers', data.data, columns, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'Users');
    },
    CreateOrUpdate: function () {
        $('.field-validation-valid').html('').hide();
        $.ajax({
            type: 'POST',
            url: $('#form-user').attr('action'),
            data: $('#form-user').serialize(),
            dataType: 'json',
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
                        window.location = publicURls.UsersList;
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
    EditUser: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.AddUser + '/?id=' + id;
    },
    DeleteUser: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        Swal.fire({
            title: globalResources.Alert_DeleteUser,
            text: '',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: globalResources.Delete,
            cancelButtonText: globalResources.Cancel,
        }).then((result) => {
            debugger;
            if (result.isConfirmed) {
                $.ajax({
                    type: 'DELETE',
                    url: publicURls.DeleteUser,
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
                                    Users.GetAllWithPager(1, GeneralClass.pageSize);
                                }
                                else {
                                    //Delete from question page.
                                    window.location.href = publicURls.UsersList;
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
    ActivationUsersConfirm: function (obj) {
        var UserId = parseInt($(obj).data("value"));
        var IsActive = $(obj).prop('checked');
        if (IsActive) {
            $('#ConfirmActivationMessage').text(globalResources.ActivationConfirm);
        }
        else {
            $('#ConfirmActivationMessage').text(globalResources.ActivationCancel);
        }

        $(obj).prop('checked', !IsActive);
        $('#modalConfirmActivation').modal();
        $('#UserId').val(UserId);
    },
    ActivateUser: function () {
        var UserId = $('#UserId').val();
        var IsActive = $('#act-user-' + UserId).prop('checked');
        IsActive = !IsActive;
        $.ajax({
            url: publicURls.ActivateUser,
            type: "post",
            data: {
                'userId': UserId,
                'isActive': IsActive
            },
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    $('#act-user-' + UserId).prop('checked', IsActive);
                    $('#UserId').val('');
                    Users.GetAllWithPager(1, GeneralClass.pageSize);
                    $('#modalConfirmActivation').modal('hide');
                }
                else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function () {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    },
    UpdateProfile: function () {
        $('.field-validation-valid').html('').hide();
        $.ajax({
            type: 'POST',
            url: $('#form-profile').attr('action'),
            data: $('#form-profile').serialize(),
            dataType: 'json',
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
                        window.location = publicURls.UsersList;
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
    }
}