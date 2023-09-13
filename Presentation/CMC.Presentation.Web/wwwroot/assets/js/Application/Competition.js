const playersInCompetition = {
    'CityMallPlayer': '',
    'OtherPlayer': ''
};

var CompetitionList = {
    OnLoad: function () {
        $(document).ready(function () {
            GeneralClass.InitalizeDatePicker('txtStartDate');
            CompetitionList.GetAllWithPager(1, GeneralClass.pageSize);
            $('#btnSearch').on('click', function () {
                $("#pagination").twbsPagination('destroy');
                CompetitionList.GetAllWithPager(1, GeneralClass.pageSize);
            });
        });
    },
    GetAllWithPager: function (pageIndex, pageSize) {
        var competitionName = $('#txtCompName').val();
        var competitonStartDate = $('#txtStartDate').val();
        var hostId = $('#ddlHosts').val();

        Grid.currentPageIndex = pageIndex;
        APIHelper.httpGet(generalSettings.BaseURL + 'Competitions/GetAllCompetitions?pageNumber=' + pageIndex +
            '&pageSize=' + pageSize +
            '&competitionName=' + competitionName +
            '&competitonStartDate=' + competitonStartDate +
            '&hostId=' + hostId
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
            { data: "competitionName", sortable: false, name: "Competition Name", autoWidth: true },
            { data: "competitionStartDate", sortable: false, name: "Competition Start Date", autoWidth: true },
            { data: "hostName", sortable: false, name: "Host Name", autoWidth: true },
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
                        $(btnUpdate).attr('onclick', 'CompetitionForm.EditCompetition(this)');
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
                        $(btndelete).attr('onclick', 'CompetitionForm.DeleteCompetition(this)');
                        $(btndelete).addClass('mx-1 action bg-danger');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-trash');
                        $(btndelete).append(pen);
                        div.append(btndelete);
                    }
                    if (CanCreate || CanDelete) {
                        return $(div).html();
                    }
                }
            }
        ];

        var columnsHost = [
            {
                data: '', sortable: false,
                render: function (data, type, row, meta) {
                    return (meta.row + meta.settings._iDisplayStart + 1) + ((currentPage - 1) * GeneralClass.pageSize);
                }
            },
            { data: "competitionName", sortable: false, name: "Competition Name", autoWidth: true },
            { data: "competitionStartDate", sortable: false, name: "Competition Start Date", autoWidth: true },
            {
                "data": "Id",
                "name": "Id",
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var div = document.createElement('div');
                    var btnStart = document.createElement('a');
                    $(btnStart).attr('id', 'btnStart_' + row.id);
                    $(btnStart).attr('onclick', 'CompetitionForm.StartCompetition(this)');
                    $(btnStart).addClass('mx-1 action bg-accent');
                    var pen = document.createElement('i');
                    $(pen).addClass('fas fa-pen');
                    $(btnStart).append(pen);
                    btnStart.textContent = globalResources.StartCompetition;
                    div.append(btnStart);
                    
                    return $(div).html();
                }
            }
        ];

        if (IsHost) {
            Grid.fillGrid('#tblCompetitions', data.data, columnsHost, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'CompetitionList');
        }
        else {
            Grid.fillGrid('#tblCompetitions', data.data, columns, true, [], '#pagination', data.totalCount, GeneralClass.pageSize, 'CompetitionList');
        }
    },
}

var CompetitionForm = {
    OnLoad: function () {
        GeneralClass.InitalizeDatePicker('StartDate', '0');
        CompetitionForm.HandleCityMallTeamDDL();
        CompetitionForm.HandleOtherTeamDDL();

        $('.js-example-basic-single').select2();

        $('.city-mall-team').on('select2:open', function (e) {
            CompetitionForm.HandleCityMallTeamDDL(e);
        });

        $('.other-team').on('select2:open', function (e) {
            CompetitionForm.HandleOtherTeamDDL(e);

        });
    },
    HandleCityMallTeamDDL: function (e) {
        let player1 = $('#Team1_Player1');
        let player2 = $('#Team1_Player2');
        let player3 = $('#Team1_Player3');
        let player4 = $('#Team1_Player4');
        if (e !== undefined) {
            debugger;
            let currentId = e.currentTarget.id;
            let $results = $('#select2-' + currentId + '-results');

            // Delay execution for 500 milliseconds (adjust as needed)
            setTimeout(function () {
                // Hide elements with matching IDs
                $results.find("li[id$='-" + player1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player4.val() + "']").css('display', 'none');
            }, 1); // Adjust the timeout duration as needed
        }
    },
    HandleOtherTeamDDL: function (e) {
        let player1 = $('#Team2_Player1');
        let player2 = $('#Team2_Player2');
        let player3 = $('#Team2_Player3');
        let player4 = $('#Team2_Player4');
        if (e !== undefined) {
            debugger;
            let currentId = e.currentTarget.id;
            let $results = $('#select2-' + currentId + '-results');

            // Delay execution for 500 milliseconds (adjust as needed)
            setTimeout(function () {
                // Hide elements with matching IDs
                $results.find("li[id$='-" + player1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player4.val() + "']").css('display', 'none');
            }, 1); // Adjust the timeout duration as needed
        }
    },
    CreateOrUpdate: function () {
        $('.field-validation-valid').hide();
        $.ajax({
            type: 'POST',
            url: $('#form-competition').attr('action'),
            data: $('#form-competition').serialize(),
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
                        window.location = publicURls.CompetitionList;
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
    EditCompetition: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.CreateNewCompetition + '/?id=' + id;
    },
    DeleteCompetition: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        Swal.fire({
            title: globalResources.Alert_DeleteCompetition,
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
                    url: publicURls.DeleteCompetition,
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
    },
    StartCompetition: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.StartCompetition + '?id=' + id;
    }
}

var StartCompetition = {
    OnLoad: function () {
        $(document).ready(function () {
            //setTimeout(function () {
            //    $('#starting-div').fadeOut(4000, function () {
            //        $('#next-content').fadeIn();
            //    });
            //}, 500); // 4 seconds

            $('.cityMallPlayers').on('click', function (e) {
                StartCompetition.SelectCityMallPlayer(e.currentTarget);
            });
            $('.OtherPlayers').on('click', function (e) {
                StartCompetition.SelectOtherTeamPlayer(e.currentTarget);
            });

            $('#img-vs').on('click', function () {
                StartCompetition.GoToPlayerVsPlayer();
            });


        });
    },
    FullScreen: function () {
        var element = document.documentElement;
        if (element.requestFullscreen) {
            element.requestFullscreen();
        } else if (element.webkitRequestFullscreen) {
            element.webkitRequestFullscreen();
        } else if (element.mozRequestFullScreen) {
            element.mozRequestFullScreen();
        } else if (element.msRequestFullscreen) {
            element.msRequestFullscreen();
        }
    },
    SelectCityMallPlayer: function (e) {
        var playerId = $(e).attr('data-player-Id');
        playersInCompetition.CityMallPlayer = playerId;
        $('.cityMallPlayers').find('.player-name').removeClass('text-player-name-c');
        $(e).find('.player-name').addClass('text-player-name-c');
    },
    SelectOtherTeamPlayer: function (e) {
        var playerId = $(e).attr('data-player-Id');
        playersInCompetition.OtherPlayer = playerId;
        $('.OtherPlayers').find('.player-name').removeClass('text-player-name-v');
        $(e).find('.player-name').addClass('text-player-name-v');
    },
    SelectOnlyOnePlayer: function (e) {
        var playerId = $(e).attr('data-player-Id');
        if ($(e).hasClass('cityMallPlayers')) {
            $('.OtherPlayers').find('.player-name').removeClass('text-player-name-v');
            $(e).find('.player-name').addClass('text-player-name-c');
            playersInCompetition.OtherPlayer = '';
            playersInCompetition.CityMallPlayer = playerId;
        }
        else {
            $('.cityMallPlayers').find('.player-name').removeClass('text-player-name-c');
            $(e).find('.player-name').addClass('text-player-name-v');
            playersInCompetition.CityMallPlayer = '';
            playersInCompetition.OtherPlayer = playerId;
        }
    },
    GoToPlayerVsPlayer: function () {
        if (playersInCompetition.CityMallPlayer == '' || playersInCompetition.OtherPlayer == '') {
            return;
        }
        var data = new FormData();
        data.append('CityMallPlayerId', parseInt(playersInCompetition.CityMallPlayer));
        data.append('OtherPlayerId', parseInt(playersInCompetition.OtherPlayer));
        $.ajax({
            type: 'POST',
            url: publicURls.PlayerVsPlayer,
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-partial').html(data.partial);
                        $('#img-goTo-category').on('click', function () {
                            StartCompetition.GoToCategories();
                        });
                        $('.cityMallPlayers').on('click', function (e) {
                            StartCompetition.SelectOnlyOnePlayer(e.currentTarget);
                        });
                        $('.OtherPlayers').on('click', function (e) {
                            StartCompetition.SelectOnlyOnePlayer(e.currentTarget);
                        });
                    }
                }
                else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    },
    GoToCategories: function () {
        if (playersInCompetition.CityMallPlayer == '' && playersInCompetition.OtherPlayer == '') {
            return;
        }
        var playerId = playersInCompetition.CityMallPlayer != '' ? parseInt(playersInCompetition.CityMallPlayer) : parseInt(playersInCompetition.OtherPlayer);
        var isCityMallPlayer = playersInCompetition.CityMallPlayer != '' ? true : false;

        var data = new FormData();
        data.append('playerId', playerId);
        data.append('IsCityMallTeam', isCityMallPlayer);
        $.ajax({
            type: 'POST',
            url: publicURls.GetCategories,
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-partial').html(data.partial);

                        $('.select-category').on('click', function (e) {
                            var elem = e.currentTarget;
                            var categoryId = $(elem).attr('data-Id');
                            StartCompetition.GoToQuestion(categoryId);
                        });
                    }
                }
                else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    },
    GoToQuestion: function (id) {

    }
}