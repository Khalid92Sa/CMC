var playersInCompetition = {
    'CityMallPlayer': '',
    'OtherPlayer': ''
};

var playerTimer = {
    'IsCityMallPlayer': '',
    'PlayerId': '',
    'IsTimerFinished': false,
    'IsTimerStarted': false,
    'IsFirstTime': true,
    'EndTime': 0,
    'IsPlayerAnswered': false, // This is to prevent duplicate confirm or submit answer, only one click on submit answer
    'IsPlayerCityMallAnswered': false,
    'IsOtherPlayerAnswered': false,
    'IsTimerFinishedAfterAnswer': false
};

var player;
var pathPlayer;

var newTimerQuestion = 0;

var CityMallPlayed = [];
var OtherTeamPlayed = [];

var CityMallFullQuestion = false;
var OtherPlayerFullQuestion = false;
var isFirstTimePlayerConfirmed = false;


var timerDiv = `<div class="h4 h4-responsive text-primary bg-light-white dv-full-timer" style="position: absolute; width: 100%; border-radius: 2.2rem !important; ">
                            <svg class="Timer" viewBox="0 0 100 100" id="timer">
                                <g id="circles">
                                    <circle cx="50" cy="50" r="20" fill="transparent" stroke="#9C59FE" stroke-width="20" />
                                    <circle cx="50" cy="50" r="20" fill="transparent" stroke="#9C59FE" stroke-width="30" id="path" />
                                </g>
                            </svg>
                        </div>`;

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
            { data: "competitionEndDate", sortable: false, name: "Competition End Date", autoWidth: true },
            { data: "hostName", sortable: false, name: "Host Name", autoWidth: true },
            {
                "data": "Id",
                "name": "Id",
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var div = document.createElement('div');

                    if (CanCreate && !row.isFinished) {
                        // If Can Create and still the competition not started, he can edit
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
                    else {
                        // Competition already finished, then only view scores.
                        var btnView = document.createElement('a');
                        $(btnView).attr('id', 'btnView_' + row.id);
                        $(btnView).attr('title', globalResources.ViewCompetitionScores);
                        $(btnView).attr('onclick', 'CompetitionForm.ViewCompetitionScores(this)');
                        $(btnView).addClass('mx-1 action bg-accent');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-eye');
                        $(btnView).append(pen);
                        div.append(btnView);
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
            { data: "competitionEndDate", sortable: false, name: "Competition End Date", autoWidth: true },
            {
                "data": "Id",
                "name": "Id",
                "autoWidth": true,
                "render": function (Id, type, row) {
                    var div = document.createElement('div');
                    if (row.isFinished) {
                        var btnView = document.createElement('a');
                        $(btnView).attr('id', 'btnView_' + row.id);
                        $(btnView).attr('onclick', 'CompetitionForm.ViewCompetitionScores(this)');
                        $(btnView).addClass('mx-1 action bg-accent');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-pen');
                        $(btnView).append(pen);
                        btnView.textContent = globalResources.ViewCompetitionScores;
                        div.append(btnView);
                    }
                    else {
                        var btnStart = document.createElement('a');
                        $(btnStart).attr('id', 'btnStart_' + row.id);
                        $(btnStart).attr('onclick', 'CompetitionForm.StartCompetition(this)');
                        $(btnStart).addClass('mx-1 action bg-accent');
                        var pen = document.createElement('i');
                        $(pen).addClass('fas fa-pen');
                        $(btnStart).append(pen);
                        btnStart.textContent = globalResources.StartCompetition;
                        div.append(btnStart);
                    }
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


        $('.teams').on('select2:open', function (e) {
            CompetitionForm.HandleDropDownListsTeams(e);
        });

        //$('.city-mall-team').on('select2:open', function (e) {
        //    CompetitionForm.HandleCityMallTeamDDL(e);
        //});

        //$('.other-team').on('select2:open', function (e) {
        //    CompetitionForm.HandleOtherTeamDDL(e);
        //});

        var competitionQuestionType = parseInt($('#CompettionQuestionType').val());
        if (IsViewCompetition) {
            competitionQuestionType = parseInt($("#hdnCompetitionQuestionType").val());
        }
        CompetitionForm.OnChangeQuestionType(competitionQuestionType);

        $('#CompettionQuestionType').on('change', function () {
            CompetitionForm.OnChangeQuestionType(parseInt($(this).val()));
        });

        $('#RoundCount').on('change', function () {
            CompetitionForm.OnRoundChange();
        });
       
    },
    OnChangeQuestionType: function (type) {
        if (type == 1) {
            //Rounds
            $('.dvRoundCount').removeClass('d-none');
            $('#dvQuestionCount').addClass('d-none');
            CompetitionForm.OnRoundChange();
        }
        else {
            //Question Count
            $('#dvQuestionCount').removeClass('d-none');
            $('.dvRoundCount').addClass('d-none');
            $('.round-2, .round-3, .round-4').addClass('d-none');
            //CompetitionForm.OnRoundChange();
        }
    },
    OnRoundChange: function () {
        var roundNum = parseInt($('#RoundCount').val());

        // Clear values in textboxes for rounds greater than roundNum
        for (var i = roundNum + 1; i <= 4; i++) {
            $('.round-' + i + ' input[type="text"]').val('');
        }

        // Show/hide round divs
        $('.round-2, .round-3, .round-4').addClass('d-none');
        for (var i = 0; i < roundNum; i++) {
            $('.round-' + (i + 1)).removeClass('d-none');
        }
    },
    HandleDropDownListsTeams: function (e) {
        let player1 = $('#Team1_Player1');
        let player2 = $('#Team1_Player2');
        let player3 = $('#Team1_Player3');
        let player4 = $('#Team1_Player4');
        let Otherplayer1 = $('#Team2_Player1');
        let Otherplayer2 = $('#Team2_Player2');
        let Otherplayer3 = $('#Team2_Player3');
        let Otherplayer4 = $('#Team2_Player4');
        if (e !== undefined) {
            let currentId = e.currentTarget.id;
            let $results = $('#select2-' + currentId + '-results');
            setTimeout(function () {
                $results.find("li[id$='-" + player1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player4.val() + "']").css('display', 'none');

                $results.find("li[id$='-" + Otherplayer1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + Otherplayer2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + Otherplayer3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + Otherplayer4.val() + "']").css('display', 'none');
            }, 1);
        }



    },
    HandleCityMallTeamDDL: function (e) {
        let player1 = $('#Team1_Player1');
        let player2 = $('#Team1_Player2');
        let player3 = $('#Team1_Player3');
        let player4 = $('#Team1_Player4');
        if (e !== undefined) {
            let currentId = e.currentTarget.id;
            let $results = $('#select2-' + currentId + '-results');
            setTimeout(function () {
                $results.find("li[id$='-" + player1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player4.val() + "']").css('display', 'none');
            }, 1);
        }
    },
    HandleOtherTeamDDL: function (e) {
        let player1 = $('#Team2_Player1');
        let player2 = $('#Team2_Player2');
        let player3 = $('#Team2_Player3');
        let player4 = $('#Team2_Player4');
        if (e !== undefined) {
            let currentId = e.currentTarget.id;
            let $results = $('#select2-' + currentId + '-results');
            setTimeout(function () {
                $results.find("li[id$='-" + player1.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player2.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player3.val() + "']").css('display', 'none');
                $results.find("li[id$='-" + player4.val() + "']").css('display', 'none');
            }, 1);
        }
    },
    CreateOrUpdate: function () {
        $('.field-validation-valid').html('').hide();
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
                                    CompetitionList.GetAllWithPager(1, GeneralClass.pageSize);
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
    },
    ViewCompetitionScores: function (e) {
        var value = $(e).attr('id');
        var id = value.split("_")[1];
        window.location.href = publicURls.ViewCompetition + '?id=' + id;
    },
    ViewPlayerScore: function (e) {
        var id = $(e).attr('data-player-Id');
        var competitionId = $(e).attr('data-compId');

        var url = publicURls.GetPlayerScoreDetails + '?competitionId=' + competitionId + '&playerId=' + id;
        window.open(url, '_blank');
    },
    ClosePlayerScoreModal: function () {
        $('#modal-data-player-details').modal('hide');
    }
}

var StartCompetition = {
    OnLoad: function () {
        $(document).ready(function () {
            if (!_isTest) {
                setTimeout(function () {
                    $('#starting-div').fadeOut(1000, function () {
                        $('#next-content').fadeIn();
                    });
                }, 5000);
            }

            $('.cityMallPlayers').on('click', function (e) {
                StartCompetition.SelectCityMallPlayer(e.currentTarget);
            });
            $('.OtherPlayers').on('click', function (e) {
                StartCompetition.SelectOtherTeamPlayer(e.currentTarget);
            });

            $('#img-vs').on('click', function () {
                StartCompetition.GoToPlayerVsPlayer();
            });

            //Check Who is from City mall && other team has already played.
            $('.cityMallPlayers').each(function (index, element) {
                var currentPlayerId = parseInt($(this).attr('data-player-Id'));
                if (CityMallPlayed.includes(currentPlayerId)) {
                    $(this).css('pointer-events', 'none');
                }
            });

            $('.OtherPlayers').each(function (index, element) {
                var currentPlayerId = parseInt($(this).attr('data-player-Id'));
                if (OtherTeamPlayed.includes(currentPlayerId)) {
                    $(this).css('pointer-events', 'none');
                }
            });
            isFirstTimePlayerConfirmed = false;
            StartCompetition.ResetObjects();
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

        if (isFinalCompetition) {
            if (playersInCompetition.CityMallPlayer == '' && playersInCompetition.OtherPlayer == '') {
                return;
            }
        }
        else {
            if (playersInCompetition.CityMallPlayer == '' || playersInCompetition.OtherPlayer == '') {
                return;
            }
        }

        var parsCityPlayer = parseInt(playersInCompetition.CityMallPlayer);
        var parsOtherPlayer = parseInt(playersInCompetition.OtherPlayer);
        var data = new FormData();
        data.append('CityMallPlayerId', parsCityPlayer);
        data.append('OtherPlayerId', parsOtherPlayer);

        CityMallPlayed.push(parsCityPlayer);
        OtherTeamPlayed.push(parsOtherPlayer);

        if (isFinalCompetition) {
            StartCompetition.GoToCategories();
        }
        else {
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

                            //$('.cityMallPlayers').on('click', function (e) {
                            //    StartCompetition.SelectOnlyOnePlayer(e.currentTarget);
                            //});
                            //$('.OtherPlayers').on('click', function (e) {
                            //    StartCompetition.SelectOnlyOnePlayer(e.currentTarget);
                            //});
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
        }
    },
    GoToCategories: function () {
        $.ajax({
            type: 'GET',
            url: publicURls.GetCategories,
            dataType: 'json',
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
        $.ajax({
            type: 'GET',
            url: publicURls.GoToQuestion + '?categoryId=' + id,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        isFirstTimePlayerConfirmed = false;
                        $('#dv-partial').html(data.partial);
                        newTimerQuestion = 0;
                        $('#btn-answer').on('click', function (e) {
                            StartCompetition.AnswerOnQuestion();
                        });

                        StartCompetition.ResetObjects();

                        $('#dv-cityMall-player, #dv-Other-player').on('click', function () {
                            var id = $(this).attr('id');

                            if (isFirstTimePlayerConfirmed == false) {
                                $('#dv-confirmPlayer').removeClass('d-none');
                                $('#btn-confirmPlayer').on('click', function () {
                                    $('#dv-confirmPlayer').addClass('d-none');
                                    StartCompetition.StartTimeForPlayer(id);
                                    $('#dv-answers-options').find('.col-6').each(function () {
                                        var element = this;
                                        $(element).removeAttr('style');
                                    });
                                });
                            }
                            else {
                                StartCompetition.StartTimeForPlayer(id);

                                $('#dv-answers-options').find('.col-6').each(function () {
                                    var element = this;
                                    $(element).removeAttr('style');
                                });
                            }
                            
                            
                        });

                        setTimeout(function () {
                            $('#dv-question-text').removeClass('d-none');
                            $('#card-main').removeAttr('style');
                        }, 1000);
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
    StartTimeForPlayer: function (id) {
        isFirstTimePlayerConfirmed = true;
        if (playerTimer.IsTimerStarted == true && playerTimer.IsTimerFinished != true) {
            return;
        }

        // Prevent to re-click on same user who answered
        var checkingPlayerId = $('#' + id).find('.hdn-cPlayer').val();
        if (checkingPlayerId == playerTimer.PlayerId) {
            return;
        }

        // Both players answeres
        if (isFinalCompetition) {
            if ((CityMallFullQuestion && id == 'dv-cityMall-player') || playerTimer.IsPlayerCityMallAnswered) {
                return;
            }
            else if ((OtherPlayerFullQuestion && id == 'dv-other-player-timer') || playerTimer.IsOtherPlayerAnswered) {
                return;
            }
        }
        else {
            // Not final competition
            if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {
                return;
            }

            if ((id == 'dv-cityMall-player' && playerTimer.IsPlayerCityMallAnswered) || (id == 'dv-Other-player' && playerTimer.IsOtherPlayerAnswered)) {
                return;
            }
        }

        var isImgAnswer = $('.img-answer-options').length > 0;
        if (isImgAnswer) {
            if ($('.img-answer-options.bg-answer-border').length > 0) {
                $('#dv-confirm').removeAttr('style');
            }
        }
        else {
            var isAnsweredSelected = $('input[name="investmentExp"]:checked');
            if (isAnsweredSelected.length > 0) {
                $('#dv-confirm').removeAttr('style');
            }
        }

        if (id == 'dv-cityMall-player') {
            $('#dv-other-player-timer').find('.dv-full-timer').remove();
            $('#dv-citymall-player-timer').append(timerDiv);
            playerTimer.IsCityMallPlayer = true;
            playerTimer.IsPlayerCityMallAnswered = true;
            playerTimer.PlayerId = $('#' + id).find('.hdn-cPlayer').val();
        } else {
            $('#dv-citymall-player-timer').find('.dv-full-timer').remove();
            $('#dv-other-player-timer').append(timerDiv);

            playerTimer.IsCityMallPlayer = false;
            playerTimer.IsOtherPlayerAnswered = true;
            playerTimer.PlayerId = $('#' + id).find('.hdn-oPlayer').val();
        }

        if (playerTimer.IsFirstTime != false) {
            playerTimer.IsFirstTime = true;
        }
        StartCompetition.StartTimer();

        playerTimer.IsTimerStarted = true;
        playerTimer.IsTimerFinished = false;
        playerTimer.IsPlayerAnswered = false;
    },
    StartTimer: function () {
        document.documentElement.classList.remove('finished');
        var questionTimer = _isTest ? 10000 : (parseInt($('#hdnQuestionTimer').val()) * 1000) + 1000;
        var duration = playerTimer.IsFirstTime ? questionTimer : 11000;
        var timer = document.getElementById('timer');
        var circles = document.getElementById('circles');
        var path = document.getElementById('path');

        var startTime = Date.now();
        playerTimer.EndTime = startTime + duration;
        $('#dv-p-timer').removeClass('d-none');
        StartCompetition.PTimer();

        if (timer.animate) {
            var pathLength = path.r.baseVal.value * 2 * Math.PI;
            player = path.animate([
                { strokeDasharray: pathLength, strokeDashoffset: pathLength },
                { strokeDasharray: pathLength, strokeDashoffset: 0 },
            ], {
                duration: duration,
                iterations: 1,
                fill: 'forwards'
            });
            circles.animate([
                { transform: 'rotate(0deg)' },
                { transform: 'rotate(-360deg)' }
            ], {
                duration: duration * 4,
                iterations: Infinity,
                fill: 'both'
            });

            var runningOutDelay = duration * .6;
            pathPlayer = timer.animate([
                { transform: 'scale(.82) rotate(-90deg)' },
                { transform: 'scale(1) rotate(-90deg)' },
            ], {
                duration: duration / 20,
                delay: runningOutDelay,
                iterations: 2,
                direction: 'alternate',
                fill: 'both'
            });

            pathPlayer.onfinish = function () {
                if (player.playState === 'running') {
                    pathPlayer.playbackRate = pathPlayer.playbackRate * 1.15;
                    pathPlayer.currentTime = runningOutDelay;
                    pathPlayer.play();
                }
            };

            var startTime = performance.now();
            player.onfinish = function () {
                if (document.documentElement.classList.contains('finished')) {
                    document.documentElement.classList.remove('finished');
                } else {
                    document.documentElement.classList.add('finished');
                }
                playerTimer.IsTimerFinished = true;
                playerTimer.IsFirstTime = false;
                player.cancel();
                pathPlayer.playbackRate = 1;
                pathPlayer.cancel();
                if (playerTimer.PlayerId != '') {
                    StartCompetition.AnswerOnQuestion();
                }
                timerFinishedSound.play();
                StartCompetition.StopTimer();
            }
        }
    },
    PTimer: function () {
        var now = Date.now();
        var remainingTime = Math.max(0, playerTimer.EndTime - now);

        var seconds = Math.floor(remainingTime / 1000);
        var minutes = Math.floor(seconds / 60);
        seconds %= 60;

        // Update the display
        $('#p-timer').text(minutes.toString().padStart(2, '0') + ':' + seconds.toString().padStart(2, '0'));

        if (remainingTime > 0) {
            newTimerQuestion++;
            requestAnimationFrame(StartCompetition.PTimer);
        }
    },
    StopTimer: function () {
        setTimeout(function () {
            player.cancel();
            pathPlayer.playbackRate = 1;
            pathPlayer.cancel();
            $('#dv-other-player-timer').find('.dv-full-timer').remove();
            $('#dv-citymall-player-timer').find('.dv-full-timer').remove();
            $('#p-timer').text('');
            $('#dv-p-timer').addClass('d-none');
            newTimerQuestion = 0;
        }, 500);
    },
    SelectAnAnswer: function (e) {
        if (playerTimer.PlayerId != '') {
            $('#dv-confirm').removeAttr('style');
        }
    },
    AnswerOnQuestion: function () {

        if (playerTimer.IsPlayerAnswered) {
            return;
        }

        var playerId = playerTimer.PlayerId;
        var isCityMall = playerTimer.IsCityMallPlayer;
        var questionId = $('#hdnQuestionId').val();
        var questionPoints = $('#hdnQuestionPoint').val();
        var answerId = null;
        var isCorrect = null;
        var answerRadio = $('input[name="investmentExp"]:checked');
        if (answerRadio.length > 0) {
            answerId = answerRadio.data('id');
            isCorrect = answerRadio.data('iscorrect');
        }

        if (playerId == 0 || playerId == '' || playerId == undefined) {
            return;
        }

        var data = new FormData();
        data.append("PlayerId", playerId);
        data.append("IsCityMallPlayer", isCityMall);
        data.append("QuestionId", questionId);
        data.append("AnswerId", answerId);
        data.append("IsCorrectAnswer", isCorrect);
        data.append("Points", questionPoints);
        data.append("Time", (newTimerQuestion / 60));

        playerTimer.IsPlayerAnswered = true;
        if (isFinalCompetition) {
            playerTimer.IsPlayerCityMallAnswered = true;
            playerTimer.IsOtherPlayerAnswered = true;
        }
        else {
            if (playerTimer.IsCityMallPlayer) {
                playerTimer.IsPlayerCityMallAnswered = true;
            }
            else {
                playerTimer.IsOtherPlayerAnswered = true;
            }
        }
        

        data.append("IsCityMallPlayerAnswered", playerTimer.IsPlayerCityMallAnswered);
        data.append("IsOtherPlayerAnswered", playerTimer.IsOtherPlayerAnswered);

        $.ajax({
            type: 'POST',
            url: publicURls.AnswerOnQuestion,
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                StartCompetition.StopTimer();
                if (data.isSuccess) {
                    newTimerQuestion = 0;

                    if (data.isFinalComp && !isRoundCompetition) {
                        if (data.correct) {
                            correctAnswerSound.play();
                            $('#goodModal').modal('show');
                        }
                        else {
                            // wrong answer
                            if (playerTimer.IsTimerFinished == true) {
                                playerTimer.IsTimerFinishedAfterAnswer = true;
                            }
                            $('#notCorrectModal').modal('show');
                            InCorrectAnswerSound.play();
                            playerTimer.IsTimerStarted = false;
                            playerTimer.IsFirstTime = false;
                        }

                        setTimeout(function () {
                            $('#goodModal, #notCorrectModal').modal('hide');


                            //In case reset both player for case both points are the same
                            if (data.reset) {
                                StartCompetition.ResetPlayed();
                                CityMallFullQuestion = false;
                                OtherPlayerFullQuestion = false;
                            }
                            else {
                                if (data.isCityMallFullQuestion) {
                                    CityMallFullQuestion = true;
                                }
                                else {
                                    CityMallPlayed = [];
                                }


                                if (data.isOtherPlayerFullQuestion) {
                                    OtherPlayerFullQuestion = true
                                }
                                else {
                                    OtherTeamPlayed = [];
                                }
                            }

                            //In case user can go to full scores
                            if (data.isScoreView) {
                                $('#dv-viewScore').find('#spn-lbl-goToScore').html(globalResources.ViewScores);
                                $('#dv-viewScore').removeAttr('style');
                                $('#dv-viewScore').on('click', function () {
                                    $('#dv-partial').html(data.scorePartial);
                                    StartCompetition.FinishFinalCompetition();
                                });


                                $('#dv-homePage').removeAttr('style');
                                $('#dv-homePage').on('click', function () {
                                    StartCompetition.OnLoad();
                                    $('#dv-partial').html(data.partial);
                                });
                            }
                            else {

                                $('#dv-homePage').removeAttr('style');
                                $('#dv-homePage').on('click', function () {
                                    StartCompetition.OnLoad();
                                    $('#dv-partial').html(data.partial);
                                });
                            }
                        }, 3000);
                    }
                    else {
                        // Not final competition Or Not questions per player..
                        if (data.correct) {
                            //Run sound effect with modal success
                            correctAnswerSound.play();
                            $('#goodModal').modal('show');
                            setTimeout(function () {
                                $('#goodModal, #notCorrectModal').modal('hide');
                                StartCompetition.OnLoad();
                                if (data.reset) {
                                    StartCompetition.ResetPlayed();
                                }

                                if (!data.finished && !data.continueRound) {
                                    //Continue to next question normal
                                    $('#dv-partial').html(data.partial);
                                }
                                else {
                                    //Competiton finish
                                    if (data.finished) {
                                        $('#dv-homePage').find('#spn-lbl-goTo').html(globalResources.ViewScores);
                                    }
                                    else if (data.continueRound) {
                                        $('#dv-homePage').find('#spn-lbl-goTo').html(data.roundText);
                                    }

                                    $('#dv-homePage').removeAttr('style');

                                    $('#dv-homePage').on('click', function () {
                                        if (data.continueRound) {
                                            StartCompetition.OnLoad();
                                            if (data.reset) {
                                                StartCompetition.ResetPlayed();
                                            }
                                        }
                                        $('#dv-partial').html(data.partial);
                                    });
                                }
                            }, 4000);
                        }
                        else {
                            if (playerTimer.IsTimerFinished == true) {
                                //InCase Timer Finished before player answered
                                //Host must clicked on another player.
                                playerTimer.IsTimerFinishedAfterAnswer = true;
                            }
                            else {
                                //Run sound effect with modal wrong
                                $('#notCorrectModal').modal('show');
                                InCorrectAnswerSound.play();
                                playerTimer.IsTimerStarted = false;
                                playerTimer.IsFirstTime = false;
                                setTimeout(function () {
                                    $('#notCorrectModal').modal('hide');
                                }, 4000);
                            }


                            if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {

                                if (data.finished) {
                                    $('#dv-homePage').find('#spn-lbl-goTo').html(globalResources.ViewScores);
                                }
                                else if (data.continueRound) {
                                    $('#dv-homePage').find('#spn-lbl-goTo').html(data.roundText);
                                }

                                //Show home page button
                                $('#dv-homePage').on('click', function () {
                                    StartCompetition.OnLoad();
                                    if (data.reset) {
                                        StartCompetition.ResetPlayed();
                                    }
                                    $('#dv-partial').html(data.partial);
                                });

                                $('#dv-homePage').removeAttr('style');

                            }
                        }
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
    ResetObjects: function () {
        playerTimer = {
            'IsCityMallPlayer': '',
            'PlayerId': '',
            'IsTimerFinished': false,
            'IsTimerStarted': false,
            'IsFirstTime': true,
            'EndTime': 0,
            'IsPlayerAnswered': false, // This is to prevent duplicate confirm or submit answer, only one click on submit answer
            'IsPlayerCityMallAnswered': false,
            'IsOtherPlayerAnswered': false,
            'IsTimerFinishedAfterAnswer': false
        };
    },
    ResetPlayed: function () {
        CityMallPlayed = [];
        OtherTeamPlayed = [];
    },
    GetFullScore: function () {
        $.ajax({
            type: 'GET',
            url: publicURls.GetFullScore,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-partial').html(data.partial);
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
    OnLoadFullScore: function () {

        $('#go-home-page-bg').on('click', function () {
            $("#loading").css("display", "flex");
            $('#next-content').fadeOut(2000, function () {
                $('#starting-div').fadeIn();
            });

            setTimeout(function () {
                window.location.href = publicURls.Logout;
            }, 5000);
               
        });
        $('.full-team-score-name').on('click', function (e) {
            var currentElem = e.currentTarget;
            var teamName = $(currentElem).attr('data-score');
            var isCityMall = teamName == 'city-mall';
            StartCompetition.GetTeamScoreDetails(isCityMall);
        });
    },
    GetTeamScoreDetails: function (isCityMall) {
        //Go to Teams scores.
        $.ajax({
            type: 'GET',
            url: publicURls.GetScoreDetails,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-partial').html(data.partial);

                        if (isCityMall) {
                            $('#dv-OtherTeamScores').hide();
                            $('#dv-CityMallScores').show();
                        }
                        else {
                            $('#dv-CityMallScores').hide();
                            $('#dv-OtherTeamScores').show();
                        }
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
    OnLoadScoresDetails: function () {
        $('.title-team-name').on('click', function (e) {
            //Get other team
            var currentElem = e.currentTarget;
            var teamName = $(currentElem).attr('data-name');
            if (teamName == 'city-mall') {
                // if user clicked on city mall, means change the values to be other team
                $('#dv-CityMallScores').hide();
                $('#dv-OtherTeamScores').show();
            }
            else {
                $('#dv-OtherTeamScores').hide();
                $('#dv-CityMallScores').show();
            }
        });

        $('#btn-back').on('click', function () {
            //Go To Full Score
            StartCompetition.GetFullScore();
        });

        //$('.player-modal-details').on('click', function (e) {
        //    var playerId = $(this).attr('data-player-Id');
        //    var isCityMall = $(this).attr('data-city-mall');
        //    StartCompetition.GetModalDetails(playerId, isCityMall);
        //});
    },
    GetModalDetails: function (playerId, isCityMall) {
        $.ajax({
            type: 'GET',
            url: publicURls.GetModalPlayer + '?playerId=' + playerId + '&isCityMall=' + isCityMall,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-modal-score').html(data.partial);
                        $('#staticBackdrop').modal('show');
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
    CloseModal: function () {
        $('#staticBackdrop').modal('hide');
    },
    FinishFinalCompetition: function () {

        $.ajax({
            type: 'GET',
            url: publicURls.FinishFinalCompetition,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                   
                }
                else {
                    //GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                //GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        }); 
    }
}