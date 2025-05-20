
var Players = {
    OnLoad: function () {
        $(document).ready(function () {
            Players.GetAllWithPager(1, GeneralClass.pageSize);
            $('#btnSearch').on('click', function () {
                $("#pagination").twbsPagination('destroy');
                Players.GetAllWithPager(1, GeneralClass.pageSize);
            });
        });
    },
    GetAllWithPager: function (pageIndex, pageSize) {
        var playerName = $('#txtPlayerName').val();
        var phoneNumber = $('#txtPhoneNumber').val();
        var playerType = $('#ddlPlayerType').val();

        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Players/GetAllPlayers?pageNumber=' + pageIndex +
            '&pageSize=' + pageSize +
            '&Name=' + playerName +
            '&PhoneNumber=' + phoneNumber +
            '&PlayerType=' + playerType
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
            { data: "name", sortable: false, name: "Player text", autoWidth: true },
            { data: "phoneNumber", sortable: false, name: "PhoneNumber", autoWidth: true },
            { data: "emailAddress", sortable: false, name: "EmailAddress", autoWidth: true },
            {
                "data": '',
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var lblElem = document.createElement('label');
                    var isEmployeeLabel = row.isEmployee ? globalResources.Yes : globalResources.No;
                    var lblText = document.createTextNode(isEmployeeLabel);
                    lblElem.appendChild(lblText);
                    var div = document.createElement('div');
                    div.append(lblElem);
                    return $(div).html();
                }
            },
            {
                "data": '',
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var lblElem = document.createElement('label');
                    var isBlockedLabel = row.isBlocked ? globalResources.Blocked : globalResources.NonBlocked;
                    var lblText = document.createTextNode(isBlockedLabel);
                    lblElem.appendChild(lblText);
                    var div = document.createElement('div');
                    div.append(lblElem);
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
                        $(btnUpdate).attr('onclick', 'Players.EditPlayer(this)');
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
                        $(btndelete).attr('onclick', 'Players.DeletePlayer(this)');
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
        Grid.fillGrid('#tblPlayers', data.data, columns, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'Players');
    },
    CreateOrUpdate: function () {
        $('.field-validation-valid').html('').hide();
        $.ajax({
            type: 'POST',
            url: $('#form-player').attr('action'),
            data: $('#form-player').serialize(),
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
    EditPlayer: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.AddNewPlayer + '/?id=' + id;
    },
    DeletePlayer: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        Swal.fire({
            title: globalResources.Alert_DeletePlayer,
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
                    url: publicURls.DeletePlayer,
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
                                    Players.GetAllWithPager(1, GeneralClass.pageSize);
                                }
                                else {
                                    //Delete from question page.
                                    window.location.href = publicURls.PlayerList;
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