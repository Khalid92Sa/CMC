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
let selectedAnswer = null;


var timerDiv = `<div class="h4 h4-responsive text-primary bg-light-white dv-full-timer" style="position: absolute; width: 100%; border-radius: 2.2rem !important; ">
                            <svg class="Timer" viewBox="0 0 100 100" id="timer">
                                <g id="circles">
                                    <circle cx="50" cy="50" r="20" fill="transparent" stroke="#9C59FE" stroke-width="20" />
                                    <circle cx="50" cy="50" r="20" fill="transparent" stroke="#9C59FE" stroke-width="30" id="path" />
                                </g>
                            </svg>
                        </div>`;

const additionalStyles = `
    <style>
        .answer-card.hover-highlight {
            border-color: #9C59FE !important;
            transform: translateY(-1px);
            box-shadow: 0 6px 20px rgba(156, 89, 254, 0.2);
        }
        
        .player-answered {
            opacity: 0.5 !important;
            pointer-events: none !important;
            cursor: not-allowed !important;
        }
        
        .animated {
            animation-duration: 0.8s;
            animation-fill-mode: both;
        }
        
        .fadeInUp {
            animation-name: fadeInUp;
        }
        
        .fadeInDown {
            animation-name: fadeInDown;
        }
        
        .fadeInUpBig {
            animation-name: fadeInUpBig;
        }

        .confirm-btn:disabled,
        .confirm-btn.disabled {
            opacity: 0.5;
            cursor: not-allowed;
            pointer-events: none;
        }
        
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translate3d(0, 40px, 0);
            }
            to {
                opacity: 1;
                transform: translate3d(0, 0, 0);
            }
        }
        
        @keyframes fadeInDown {
            from {
                opacity: 0;
                transform: translate3d(0, -40px, 0);
            }
            to {
                opacity: 1;
                transform: translate3d(0, 0, 0);
            }
        }
        
        @keyframes fadeInUpBig {
            from {
                opacity: 0;
                transform: translate3d(0, 60px, 0);
            }
            to {
                opacity: 1;
                transform: translate3d(0, 0, 0);
            }
        }
    </style>
`;


var gameState = {
    INITIAL: 'initial',           // Just loaded, show question button
    QUESTION_SHOWN: 'question_shown', // Question visible, waiting for player selection
    PLAYER_SELECTED: 'player_selected', // Player selected, waiting for continue to show options
    OPTIONS_SHOWN: 'options_shown',     // Options visible, timer running
    TIMER_FINISHED: 'timer_finished',  // Timer finished, waiting for manual answer submission
    QUESTION_FINISHED: 'question_finished' // Question completed, players/options disabled
};

var currentGameState = gameState.INITIAL;
var selectedPlayerId = null;
var isQuestionFinished = false;


var CompetitionList = {
    OnLoad: function () {
        GeneralClass.InitalizeDatePicker('txtStartDate');
        CompetitionList.GetAllWithPager(1, GeneralClass.pageSize);
        $('#btnSearch').on('click', function () {
            $("#pagination").twbsPagination('destroy');
            CompetitionList.GetAllWithPager(1, GeneralClass.pageSize);
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
        GeneralClass.InitalizeDatePicker('ArchiveFromDate', '0');
        GeneralClass.InitalizeDatePicker('ArchiveToDate', '0');
        CompetitionForm.HandleCityMallTeamDDL();
        CompetitionForm.HandleOtherTeamDDL();

        $('.js-example-basic-single').select2();


        $('.teams').on('select2:open', function (e) {
            CompetitionForm.HandleDropDownListsTeams(e);
        });

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

        $('#ArchiveType').on('change', function () {
            CompetitionForm.OnArchiveTypeChange();
        });
    },
    ArchiveTypes: {
        None: 0,
        TimeBased: 1,
        CompetitionBased: 2,
        Global: 3,
        DateRange: 4
    },
    OnArchiveTypeChange: function () {
        var archiveType = $('#ArchiveType').find('option:selected').attr('data-code');

        // Hide all archive-specific fields
        $('.archive-field').addClass('d-none');
        $('#archivePreview').addClass('d-none');

        // Show relevant fields based on archive type
        switch (archiveType) {
            case '1': // TimeBased
                $('.archive-field.time-based').removeClass('d-none');
                break;
            case '2': // CompetitionBased
                $('.archive-field.competition-based').removeClass('d-none');
                break;
            case '4': // DateRange
                $('.archive-field.date-range').removeClass('d-none');
                break;
            case '3': // Global
                $('#archivePreviewText').text(globalResources.AllQuestionsFromPreviousCompetitionsWillBeExcluded);
                $('#archivePreview').removeClass('d-none');
                break;
            case '0': // None
            default:
                $('#archivePreviewText').text(globalResources.OnlyQuestionsFromParentCompetitionsWillBeExcluded);
                $('#archivePreview').removeClass('d-none');
                break;
        }
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
        if (!_isTest) {
            setTimeout(function () {
                $('#starting-div').fadeOut(1000, function () {
                    $('#next-content').fadeIn();
                });
            }, 2000);
        }

        playersInCompetition.CityMallPlayer = '';
        playersInCompetition.OtherPlayer = '';

        // Handle final competition setup
        if (isFinalCompetition) {
            StartCompetition.SetupFinalCompetition();
        } else {
            StartCompetition.SetupRegularCompetition();
        }

        $('#battle-back-btn').on('click', function () {
            StartCompetition.GoBackFromBattleMode();
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
    },
    SetupFinalCompetition: function () {
        // In final competition, automatically set the players and enable VS button
        var cityMallPlayer = $('.team-section').first().find('.player-card').attr('data-player-Id');
        var otherPlayer = $('.team-section').last().find('.player-card').attr('data-player-Id');

        if (cityMallPlayer) {
            playersInCompetition.CityMallPlayer = cityMallPlayer;
        }
        if (otherPlayer) {
            playersInCompetition.OtherPlayer = otherPlayer;
        }

        // VS button is already active via CSS, no need to manually activate
        isBattleMode = true;
    },

    SetupRegularCompetition: function () {
        // Regular competition setup
        $('.cityMallPlayers').on('click', function (e) {
            StartCompetition.SelectCityMallPlayer(e.currentTarget);
        });
        $('.OtherPlayers').on('click', function (e) {
            StartCompetition.SelectOtherTeamPlayer(e.currentTarget);
        });

        isBattleMode = false;
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

        // Remove selection from all City Mall players
        $('.cityMallPlayers').removeClass('selected');
        $('.cityMallPlayers').find('.player-name').removeClass('text-player-name-c');

        // Add selection to clicked player
        $(e).addClass('selected');
        $(e).find('.player-name').addClass('text-player-name-c');

        // For final competition, clear other team selection if this player is selected
        if (isFinalCompetition) {
            $('.OtherPlayers').removeClass('selected');
            $('.OtherPlayers').find('.player-name').removeClass('text-player-name-v');
            playersInCompetition.OtherPlayer = '';
        }

        // Check if we should activate battle mode
        StartCompetition.CheckBattleMode();
    },
    SelectOtherTeamPlayer: function (e) {
        var playerId = $(e).attr('data-player-Id');
        playersInCompetition.OtherPlayer = playerId;

        // Remove selection from all Other team players
        $('.OtherPlayers').removeClass('selected');
        $('.OtherPlayers').find('.player-name').removeClass('text-player-name-v');

        // Add selection to clicked player
        $(e).addClass('selected');
        $(e).find('.player-name').addClass('text-player-name-v');

        // For final competition, clear city mall selection if this player is selected
        if (isFinalCompetition) {
            $('.cityMallPlayers').removeClass('selected');
            $('.cityMallPlayers').find('.player-name').removeClass('text-player-name-c');
            playersInCompetition.CityMallPlayer = '';
        }

        // Check if we should activate battle mode
        StartCompetition.CheckBattleMode();
    },
    GoBackFromBattleMode: function () {
        // Reset battle mode
        isBattleMode = false;

        // Remove battle mode class
        $('.competition-home-container').removeClass('battle-mode');

        // Clear player selections
        $('.player-card').removeClass('selected');
        $('.player-name').removeClass('text-player-name-c text-player-name-v');

        // Reset player selection variables
        playersInCompetition.CityMallPlayer = '';
        playersInCompetition.OtherPlayer = '';

        // Reset avatar animations
        $('.player-avatar').css('animation-name', 'pulse');

        // Optional: Show a brief message
        // You can add a toast notification here if you want
    },
    SelectOnlyOnePlayer: function (e) {
        var playerId = $(e).attr('data-player-Id');
        if ($(e).hasClass('cityMallPlayers')) {
            $('.OtherPlayers').find('.player-name').removeClass('text-player-name-v');
            $('.OtherPlayers').removeClass('selected');
            $(e).find('.player-name').addClass('text-player-name-c');
            $(e).addClass('selected');
            playersInCompetition.OtherPlayer = '';
            playersInCompetition.CityMallPlayer = playerId;
        }
        else {
            $('.cityMallPlayers').find('.player-name').removeClass('text-player-name-c');
            $('.cityMallPlayers').removeClass('selected');
            $(e).find('.player-name').addClass('text-player-name-v');
            $(e).addClass('selected');
            playersInCompetition.CityMallPlayer = '';
            playersInCompetition.OtherPlayer = playerId;
        }

        // Check if we should activate battle mode
        StartCompetition.CheckBattleMode();
    },
    CheckBattleMode: function () {
        var shouldActivateBattleMode = false;

        if (isFinalCompetition) {
            // For final competition, activate battle mode if at least one player is selected
            shouldActivateBattleMode = (playersInCompetition.CityMallPlayer !== '' && playersInCompetition.CityMallPlayer !== undefined) ||
                (playersInCompetition.OtherPlayer !== '' && playersInCompetition.OtherPlayer !== undefined);
        } else {
            // For regular competition, both players must be selected
            shouldActivateBattleMode = (playersInCompetition.CityMallPlayer !== '' && playersInCompetition.CityMallPlayer !== undefined) &&
                (playersInCompetition.OtherPlayer !== '' && playersInCompetition.OtherPlayer !== undefined);
        }

        if (shouldActivateBattleMode && !isBattleMode) {
            StartCompetition.ActivateBattleMode();
        } else if (!shouldActivateBattleMode && isBattleMode) {
            StartCompetition.DeactivateBattleMode();
        }
    },
    ActivateBattleMode: function () {
        isBattleMode = true;

        // Add battle mode class to main container
        $('.competition-home-container').addClass('battle-mode');

        // Add final competition class if needed
        if (isFinalCompetition) {
            $('.competition-home-container').addClass('final-competition');

            // For final competition, show VS button as active immediately
            $('#img-vs').addClass('active');

            // Show only team sections that have selected players
            $('.team-section').removeClass('has-selected-player');
            var selectedTeams = 0;

            if (playersInCompetition.CityMallPlayer !== '' && playersInCompetition.CityMallPlayer !== undefined) {
                $('.team-section').first().addClass('has-selected-player');
                selectedTeams++;
            }
            if (playersInCompetition.OtherPlayer !== '' && playersInCompetition.OtherPlayer !== undefined) {
                $('.team-section').last().addClass('has-selected-player');
                selectedTeams++;
            }

            // Always show single player centered in final competition
            StartCompetition.UpdateFinalCompetitionLayout();

        } else {
            // Regular competition - show VS as active only when both players selected
            if ((playersInCompetition.CityMallPlayer !== '' && playersInCompetition.CityMallPlayer !== undefined) &&
                (playersInCompetition.OtherPlayer !== '' && playersInCompetition.OtherPlayer !== undefined)) {
                $('#img-vs').addClass('active');
            }
        }

        // After transition completes, add battle animations
        setTimeout(function () {
            // Add ripple effects to selected avatars
            $('.player-card.selected .player-avatar').css('animation-name', 'battlePulse');
        }, 800);
    },
    DeactivateBattleMode: function () {
        isBattleMode = false;
        $('.competition-home-container').removeClass('battle-mode');
        // Only remove final-competition class if it's not actually a final competition
        if (!isFinalCompetition) {
            $('.competition-home-container').removeClass('final-competition');
        }
        $('.player-avatar').css('animation-name', 'pulse'); // Reset to default animation
        $('#img-vs').removeClass('active');
        $('.team-section').removeClass('has-selected-player');
    },
    UpdateFinalCompetitionLayout: function () {
        if (!isFinalCompetition) return;

        var $container = $('.teams-container');
        var $vsSection = $('.vs-section');

        // In final competition, always center the selected player with VS button
        $container.addClass('single-player-mode');

        // Always show VS section in final competition
        $vsSection.show();

        // Center the layout
        $container.css({
            'justify-content': 'center',
            'gap': '80px'
        });
    },
    GoToPlayerVsPlayer: function () {
        // Check if VS button should be clickable
        if (isFinalCompetition) {
            // Final competition - VS is always clickable
            var parsCityPlayer = parseInt(playersInCompetition.CityMallPlayer || 0);
            var parsOtherPlayer = parseInt(playersInCompetition.OtherPlayer || 0);

            var data = new FormData();
            data.append('CityMallPlayerId', parsCityPlayer);
            data.append('OtherPlayerId', parsOtherPlayer);

            // Add to played arrays if they have valid IDs
            if (parsCityPlayer > 0) {
                CityMallPlayed.push(parsCityPlayer);
            }
            if (parsOtherPlayer > 0) {
                OtherTeamPlayed.push(parsOtherPlayer);
            }

            StartCompetition.GoToCategories();
        } else {
            // Regular competition - check battle mode activation
            if (!isBattleMode) {
                return;
            }

            // Both players must be selected
            if ((playersInCompetition.CityMallPlayer === '' || playersInCompetition.CityMallPlayer === undefined) ||
                (playersInCompetition.OtherPlayer === '' || playersInCompetition.OtherPlayer === undefined)) {
                return;
            }

            var parsCityPlayer = parseInt(playersInCompetition.CityMallPlayer || 0);
            var parsOtherPlayer = parseInt(playersInCompetition.OtherPlayer || 0);
            var data = new FormData();
            data.append('CityMallPlayerId', parsCityPlayer);
            data.append('OtherPlayerId', parsOtherPlayer);

            // Add to played arrays
            if (parsCityPlayer > 0) {
                CityMallPlayed.push(parsCityPlayer);
            }
            if (parsOtherPlayer > 0) {
                OtherTeamPlayed.push(parsOtherPlayer);
            }

            $.ajax({
                type: 'POST',
                url: publicURls.PlayerVsPlayer,
                data: data,
                contentType: false,
                processData: false,
                success: function (data) {
                    if (data.isSuccess) {
                        StartCompetition.GoToCategories();
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
    ResetBattleMode: function () {
        isBattleMode = false;
        $('.competition-home-container').removeClass('battle-mode');
        $('.player-card').removeClass('selected');
        $('.player-name').removeClass('text-player-name-c text-player-name-v');
        $('.player-avatar').css('animation-name', 'pulse'); // Reset to default animation

        // Reset player selections
        playersInCompetition.CityMallPlayer = '';
        playersInCompetition.OtherPlayer = '';
    },
    GoToQuestion: function (id) {
        $.ajax({
            type: 'GET',
            url: publicURls.GoToQuestion + '?categoryId=' + id,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        $('#dv-partial').html(data.partial);
                        selectedAnswer = null;
                        StartCompetition.RenderEventOnGetQuestion();
                        $('#btn-change-question').attr('data', 'val-categoryId').val(id);
                        $('#btn-change-question').on('click', function (e) {
                            StartCompetition.ChangeQuestion();
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
    RenderEventOnGetQuestion: function () {
        newTimerQuestion = 0;
        selectedAnswer = null;
        currentGameState = gameState.INITIAL;
        selectedPlayerId = null;
        isQuestionFinished = false;

        $('#btn-answer').off('click').on('click', function (e) {
            StartCompetition.HandleContinueButton();
        });

        $('#btn-change-question').off('click').on('click', function (e) {
            if (currentGameState === gameState.QUESTION_SHOWN || currentGameState === gameState.PLAYER_SELECTED) {
                StartCompetition.ChangeQuestion();
            }
        });

        $(document).off('mouseenter mouseleave', '.answer-card');
        $(document).on('mouseenter', '.answer-card', function () {
            if ((currentGameState === gameState.OPTIONS_SHOWN || currentGameState === gameState.TIMER_FINISHED) && !$(this).hasClass('selected') && !isQuestionFinished) {
                $(this).addClass('hover-highlight');
            }
        });

        $(document).on('mouseleave', '.answer-card', function () {
            $(this).removeClass('hover-highlight');
        });

        // Handle answer selection when options are shown OR timer finished (but question not finished)
        $(document).off('click', 'input[name="investmentExp"]');
        $(document).on('click', 'input[name="investmentExp"]', function () {
            if ((currentGameState === gameState.OPTIONS_SHOWN || currentGameState === gameState.TIMER_FINISHED) && !isQuestionFinished) {
                $('.answer-card').removeClass('selected');
                $(this).closest('.answer-option').find('.answer-card').addClass('selected');
                StartCompetition.SelectAnAnswer(this);
            }
        });

        $(document).off('click', '.answer-card');
        $(document).on('click', '.answer-card', function () {
            if ((currentGameState === gameState.OPTIONS_SHOWN || currentGameState === gameState.TIMER_FINISHED) && !isQuestionFinished) {
                StartCompetition.selectAnswer(this);
            }
        });

        // Image answer options handler
        $(document).off('click', '.img-answer-options');
        $(document).on('click', '.img-answer-options', function () {
            if ((currentGameState === gameState.OPTIONS_SHOWN || currentGameState === gameState.TIMER_FINISHED) && !isQuestionFinished) {
                $(this).toggleClass('bg-answer-border');
                $('.img-answer-options').not(this).removeClass('bg-answer-border');
                StartCompetition.CheckIfAnswerSelected();
            }
        });

        $('head').append(additionalStyles);
        StartCompetition.ResetObjects();

        // Player click handlers - allow selection when question is shown OR when player is already selected (to change selection)
        $('#dv-cityMall-player, #dv-Other-player').off('click').on('click', function () {
            if ((currentGameState === gameState.QUESTION_SHOWN ||
                currentGameState === gameState.PLAYER_SELECTED ||
                currentGameState === gameState.TIMER_FINISHED) && !isQuestionFinished) {
                StartCompetition.SelectPlayer($(this).attr('id'));
            }
        });

        // Show main card and continue button initially
        setTimeout(function () {
            $('#card-main').css('visibility', 'visible').addClass('animated fadeInUpBig');
            $('#dv-confirm').css('visibility', 'visible');
            $('#btn-answer').prop('disabled', false).removeClass('disabled');
            StartCompetition.UpdateContinueButtonText();
        }, 1000);
    },
    UpdateContinueButtonText: function () {
        var buttonText = '';
        if (globalResources != undefined) {
            switch (currentGameState) {
                case gameState.INITIAL:
                    buttonText = globalResources.Continue;
                    break;
                case gameState.QUESTION_SHOWN:
                    buttonText = selectedPlayerId ? globalResources.ShowOptions : globalResources.SelectPlayerFirst;
                    break;
                case gameState.PLAYER_SELECTED:
                    buttonText = globalResources.ShowOptions;
                    break;
                case gameState.OPTIONS_SHOWN:
                    buttonText = globalResources.SubmitAnswer;
                    break;
                case gameState.TIMER_FINISHED:
                    buttonText = globalResources.SubmitAnswer;
                    break;
                default:
                    buttonText = globalResources.Continue;
            }
        }

        $('#btn-answer .button-text').text(buttonText);
    },
    HandleContinueButton: function () {
        switch (currentGameState) {
            case gameState.INITIAL:
                // Show the question
                StartCompetition.ShowQuestion();
                break;

            case gameState.QUESTION_SHOWN:
                // Need player selection first
                if (!selectedPlayerId) {
                    // Show message or do nothing - button should be disabled
                    return;
                }
                // Player is selected, show options
                StartCompetition.ShowOptionsAndStartTimer();
                break;

            case gameState.PLAYER_SELECTED:
                // Show options and start timer
                StartCompetition.ShowOptionsAndStartTimer();
                break;

            case gameState.OPTIONS_SHOWN:
                // Submit answer if one is selected
                if (StartCompetition.HasAnswerSelected()) {
                    StartCompetition.AnswerOnQuestion();
                }
                break;

            case gameState.TIMER_FINISHED:
                // Submit answer (could be empty if no answer selected)
                StartCompetition.AnswerOnQuestion();
                break;

            default:
                break;
        }
    },
    SelectPlayer: function (playerId) {
        if (isQuestionFinished) {
            return;
        }

        // Check if this player has already answered
        var playerAlreadyAnswered = false;
        if (playerId === 'dv-cityMall-player' && playerTimer.IsPlayerCityMallAnswered) {
            playerAlreadyAnswered = true;
        } else if (playerId === 'dv-Other-player' && playerTimer.IsOtherPlayerAnswered) {
            playerAlreadyAnswered = true;
        }

        // Don't allow selecting a player who already answered
        if (playerAlreadyAnswered) {
            return;
        }

        // Don't allow selecting the same player who just finished answering
        if (selectedPlayerId === playerId && playerTimer.IsPlayerAnswered) {
            return;
        }

        selectedPlayerId = playerId;
        StartCompetition.activatePlayer(playerId);

        // Set player timer info but DON'T set answered flags yet
        if (playerId === 'dv-cityMall-player') {
            playerTimer.IsCityMallPlayer = true;
            playerTimer.PlayerId = $('#' + playerId).find('.hdn-cPlayer').val();
        } else {
            playerTimer.IsCityMallPlayer = false;
            playerTimer.PlayerId = $('#' + playerId).find('.hdn-oPlayer').val();
        }

        // Reset player answered flag for current attempt
        playerTimer.IsPlayerAnswered = false;

        // Check if options are already visible (second player selection)
        var optionsAlreadyVisible = $('#dv-answers-options').find('.answer-option[style*="visible"]').length > 0;

        if (optionsAlreadyVisible) {
            // Options already shown, go directly to options state
            currentGameState = gameState.OPTIONS_SHOWN;

            // Make sure continue button is visible for second player
            $('#dv-confirm').css('visibility', 'visible');

            // Re-enable answer options for second player
            $('.answer-card, .img-answer-options').css('pointer-events', 'auto').removeClass('disabled');
            $('input[name="investmentExp"]').prop('disabled', false);

            // Disable change question button when timer starts for second player (don't hide, just disable)
            $('#btn-change-question').prop('disabled', true).addClass('disabled');

            // Start timer immediately for second player
            setTimeout(function () {
                StartCompetition.StartTimer();
                playerTimer.IsTimerStarted = true;
                playerTimer.IsTimerFinished = false;
                playerTimer.IsPlayerAnswered = false;
            }, 500);

            // Enable submit button if answer already selected, otherwise disable
            if (StartCompetition.HasAnswerSelected()) {
                $('#btn-answer').prop('disabled', false).removeClass('disabled');
            } else {
                $('#btn-answer').prop('disabled', true).addClass('disabled');
            }
        } else {
            // First time, need to click continue to show options
            currentGameState = gameState.PLAYER_SELECTED;
            $('#btn-answer').prop('disabled', false).removeClass('disabled');
        }

        StartCompetition.UpdateContinueButtonText();
    },
    MarkPlayerAsAnswered: function (playerId) {
        // Add visual indication that this player has already answered
        $('#' + playerId).addClass('player-answered').css('opacity', '0.5');
    },
    ShowOptionsAndStartTimer: function () {
        if (!selectedPlayerId || isQuestionFinished) {
            return;
        }

        currentGameState = gameState.OPTIONS_SHOWN;

        // Disable change question button when timer starts
        $('#btn-change-question').prop('disabled', true).addClass('disabled');

        // Show answer options with animation
        setTimeout(function () {
            $('#dv-answers-options').find('.answer-option').each(function (index) {
                var element = this;
                setTimeout(function () {
                    $(element).css('visibility', 'visible').addClass('animated fadeInUp');
                }, index * 200);
            });
        }, 300);

        // Start timer after answers are shown
        setTimeout(function () {
            StartCompetition.StartTimer();
            playerTimer.IsTimerStarted = true;
            playerTimer.IsTimerFinished = false;
            playerTimer.IsPlayerAnswered = false;
        }, 800);

        StartCompetition.UpdateContinueButtonText();
        $('#btn-answer').prop('disabled', true).addClass('disabled');
    },
    HasAnswerSelected: function () {
        var isImgAnswer = $('.img-answer-options').length > 0;
        if (isImgAnswer) {
            return $('.img-answer-options.bg-answer-border').length > 0;
        } else {
            return $('input[name="investmentExp"]:checked').length > 0;
        }
    },
    ChangeQuestion: function () {
        debugger;
        var categoryId = $('#btn-change-question').attr('data', 'val-categoryId').val();
        $.ajax({
            type: 'GET',
            url: publicURls.ChangeQuestion + '?categoryId=' + categoryId,
            dataType: 'json',
            success: function (data) {
                if (data.isSuccess) {
                    if (data.partial != '') {
                        // Replace the question content
                        $('#question-content-container').html(data.partial);

                        StartCompetition.RenderEventOnGetQuestion();

                        $('#btn-answer').trigger('click');
                    }
                }
                else {
                    GeneralClass.ShowErrorAlert(data.message || globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    },
    ShowQuestion: function () {
        $('#dv-question-text').show().addClass('animated fadeInDown');
        $('#dv-change-question').css('visibility', 'visible');

        currentGameState = gameState.QUESTION_SHOWN;
        StartCompetition.UpdateContinueButtonText();

        // Keep continue button disabled until player is selected
        $('#btn-answer').prop('disabled', true).addClass('disabled');

        // Make sure players are clickable
        $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'auto').removeClass('disabled');
    },
    StartTimeForPlayer: function (id) {
        // CHANGE: Only proceed if question is visible
        if ($('#dv-question-text').is(':hidden')) {
            return; // Question not shown yet
        }

        StartCompetition.activatePlayer(id);
        isFirstTimePlayerConfirmed = true;

        if (playerTimer.IsTimerStarted == true && playerTimer.IsTimerFinished != true) {
            return;
        }

        // Prevent to re-click on same user who answered
        var checkingPlayerId = $('#' + id).find('.hdn-cPlayer').val();
        if (checkingPlayerId == playerTimer.PlayerId) {
            return;
        }

        // Both players answers logic
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
            playerTimer.IsCityMallPlayer = true;
            playerTimer.IsPlayerCityMallAnswered = true;
            playerTimer.PlayerId = $('#' + id).find('.hdn-cPlayer').val();
        } else {
            playerTimer.IsCityMallPlayer = false;
            playerTimer.IsOtherPlayerAnswered = true;
            playerTimer.PlayerId = $('#' + id).find('.hdn-oPlayer').val();
        }

        if (playerTimer.IsFirstTime != false) {
            playerTimer.IsFirstTime = true;
        }

        // CHANGE: Show answer options with animation after player selection
        setTimeout(function () {
            $('#dv-answers-options').find('.answer-option').each(function (index) {
                var element = this;
                setTimeout(function () {
                    $(element).css('visibility', 'visible').addClass('animated fadeInUp');
                }, index * 200); // 200ms delay between each answer
            });
        }, 300);

        // START TIMER after answers are shown
        setTimeout(function () {
            StartCompetition.StartTimer();
            playerTimer.IsTimerStarted = true;
            playerTimer.IsTimerFinished = false;
            playerTimer.IsPlayerAnswered = false;
        }, 800);
    },
    StartTimer: function () {
        document.documentElement.classList.remove('finished');
        var questionTimer = _isTest ? 10000 : (parseInt($('#hdnQuestionTimer').val()) * 1000) + 1000;
        var duration = playerTimer.IsFirstTime ? questionTimer : 11000;

        var startTime = Date.now();
        playerTimer.EndTime = startTime + duration;

        $('#dv-p-timer').removeClass('d-none');
        StartCompetition.PTimer();

        var timerInterval = setTimeout(function () {
            if (document.documentElement.classList.contains('finished')) {
                document.documentElement.classList.remove('finished');
            } else {
                document.documentElement.classList.add('finished');
            }

            playerTimer.IsTimerFinished = true;
            playerTimer.IsFirstTime = false;

            // Set answered flag when timer finishes so player can't be selected again
            if (playerTimer.IsCityMallPlayer) {
                playerTimer.IsPlayerCityMallAnswered = true;
            } else {
                playerTimer.IsOtherPlayerAnswered = true;
            }

            // Play timer finished sound but don't auto-submit
            if (timerFinishedSound) {
                timerFinishedSound.play();
            }

            // Change state to timer finished and update button
            currentGameState = gameState.TIMER_FINISHED;
            StartCompetition.UpdateContinueButtonText();
            $('#btn-answer').prop('disabled', false).removeClass('disabled');

            // Keep answer options clickable even after timer finishes
            $('.answer-card, .img-answer-options').css('pointer-events', 'auto').removeClass('disabled');
            $('input[name="investmentExp"]').prop('disabled', false);

            // Check if both players have now answered (including timer finish)
            if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {
                // Both players answered - disable all player interactions but keep options clickable
                $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'none').addClass('disabled');
            } else {
                // Keep players clickable only if they haven't answered yet
                $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'auto').removeClass('disabled');
            }
        }, duration);

        playerTimer.TimerInterval = timerInterval;
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
            // Clear any running timer
            if (playerTimer.TimerInterval) {
                clearTimeout(playerTimer.TimerInterval);
                playerTimer.TimerInterval = null;
            }

            // Hide the numeric timer
            $('#dv-p-timer').addClass('d-none');
            $('#p-timer').text('');

            // Clear answer selection when timer stops
            document.querySelectorAll('.answer-card').forEach(card => {
                card.classList.remove('selected');
            });

            // CHANGE: Don't hide confirm button, just disable it and check state
            // $('#dv-confirm').css('visibility', 'hidden'); // REMOVED THIS LINE
            $('#btn-answer').prop('disabled', true).addClass('disabled');

            newTimerQuestion = 0;
        }, 500);
    },
    selectAnswer: function (card) {
        // Remove selection from all cards
        document.querySelectorAll('.answer-card').forEach(c => {
            c.classList.remove('selected');
        });

        // Add selection to clicked card
        card.classList.add('selected');
        selectedAnswer = card;

        // Trigger the original radio button for compatibility
        const radio = card.parentElement.querySelector('input[type="radio"]');
        if (radio) {
            radio.checked = true;
            StartCompetition.SelectAnAnswer(radio);
        }
    },
    activatePlayer: function (playerId) {
        if (isQuestionFinished) {
            return;
        }

        // Remove active class from both players
        document.querySelectorAll('.player-section').forEach(player => {
            player.classList.remove('active', 'correct', 'wrong');
        });

        // Add active class to selected player
        const activePlayer = document.getElementById(playerId);
        if (activePlayer) {
            activePlayer.classList.add('active');
        }
    },
    showCorrectAnswerFeedback: function () {
        // Get the active player
        const activePlayer = document.querySelector('.player-section.active');
        if (activePlayer) {
            activePlayer.classList.remove('active');
            activePlayer.classList.add('correct');
        }

        // Get the selected answer
        const selectedAnswerCard = document.querySelector('.answer-card.selected');
        if (selectedAnswerCard) {
            selectedAnswerCard.classList.remove('selected');
            selectedAnswerCard.classList.add('correct-answer');
        }

        // Change timer color to green
        const timer = document.getElementById('dv-p-timer');
        if (timer) {
            timer.classList.add('correct');
        }

        // Reset after 5 seconds
        setTimeout(() => {
            if (activePlayer) activePlayer.classList.remove('correct');
            if (selectedAnswerCard) selectedAnswerCard.classList.remove('correct-answer');
            if (timer) timer.classList.remove('correct');
        }, 5000);
    },
    showWrongAnswerFeedback: function () {
        // Get the active player
        const activePlayer = document.querySelector('.player-section.active');
        if (activePlayer) {
            activePlayer.classList.remove('active');
            activePlayer.classList.add('wrong');
        }

        // Get the selected answer
        const selectedAnswerCard = document.querySelector('.answer-card.selected');
        if (selectedAnswerCard) {
            selectedAnswerCard.classList.remove('selected');
            selectedAnswerCard.classList.add('wrong-answer');
        }

        // Change timer color to red
        const timer = document.getElementById('dv-p-timer');
        if (timer) {
            timer.classList.add('wrong');
        }

        // Reset after 5 seconds
        setTimeout(() => {
            if (activePlayer) activePlayer.classList.remove('wrong');
            if (selectedAnswerCard) selectedAnswerCard.classList.remove('wrong-answer');
            if (timer) timer.classList.remove('wrong');
        }, 5000);
    },
    SelectAnAnswer: function (e) {
        if (currentGameState === gameState.OPTIONS_SHOWN && !isQuestionFinished) {
            StartCompetition.CheckIfAnswerSelected();
        }
    },
    CheckIfAnswerSelected: function () {
        if ((currentGameState === gameState.OPTIONS_SHOWN || currentGameState === gameState.TIMER_FINISHED) && !isQuestionFinished) {
            if (StartCompetition.HasAnswerSelected()) {
                $('#btn-answer').prop('disabled', false).removeClass('disabled');
            } else if (currentGameState === gameState.OPTIONS_SHOWN) {
                $('#btn-answer').prop('disabled', true).addClass('disabled');
            }
            // If timer finished, keep button enabled regardless of answer selection
        }
    },
    CheckButtonStateAfterAnswer: function () {
        // Check if question is visible
        if ($('#dv-question-text').is(':visible')) {
            // Question is visible, check if we can enable button for next player
            var hasAnswer = false;
            var hasPlayer = playerTimer.PlayerId != '' && playerTimer.PlayerId != undefined && playerTimer.PlayerId != '0';

            // Check if answer is selected
            var isImgAnswer = $('.img-answer-options').length > 0;
            if (isImgAnswer) {
                hasAnswer = $('.img-answer-options.bg-answer-border').length > 0;
            } else {
                hasAnswer = $('input[name="investmentExp"]:checked').length > 0;
            }

            // Enable button only if both player and answer are selected
            if (hasAnswer && hasPlayer) {
                $('#btn-answer').prop('disabled', false).removeClass('disabled');
            } else {
                $('#btn-answer').prop('disabled', true).addClass('disabled');
            }
        } else {
            // Question not visible, enable button to show question
            $('#btn-answer').prop('disabled', false).removeClass('disabled');
        }
    },
    AnswerOnQuestion: function () {

        if (playerTimer.IsPlayerAnswered || isQuestionFinished) {
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
        if (playerTimer.IsCityMallPlayer) {
            playerTimer.IsPlayerCityMallAnswered = true;
        } else {
            playerTimer.IsOtherPlayerAnswered = true;
        }


        // Check if both players have answered or if it's final competition
        //var bothPlayersAnswered = (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered);

        //if (bothPlayersAnswered || isFinalCompetition) {
        //    isQuestionFinished = true;
        //    currentGameState = gameState.QUESTION_FINISHED;
        //    StartCompetition.DisableAllInteractions();

        //    playerTimer.IsPlayerCityMallAnswered = true;
        //    playerTimer.IsOtherPlayerAnswered = true;

        //} else {
        //    // Allow selecting another player, keep options visible
        //    currentGameState = gameState.QUESTION_SHOWN;
        //    selectedPlayerId = null; // Reset selected player

        //    // Re-enable player selection and keep options visible
        //    $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'auto').removeClass('disabled');

        //    // Clear only the selection, keep options visible
        //    $('.answer-card').removeClass('selected correct-answer wrong-answer');
        //    $('input[name="investmentExp"]').prop('checked', false).prop('disabled', false);
        //    $('.img-answer-options').removeClass('bg-answer-border');

        //    // Reset timer display but don't restart it
        //    $('#dv-p-timer').addClass('d-none');

        //    // Update button text and disable until player selected
        //    StartCompetition.UpdateContinueButtonText();
        //    $('#btn-answer').prop('disabled', true).addClass('disabled');

        //    // Show change question button again
        //    $('#dv-change-question').css('visibility', 'visible');
        //}

        isQuestionFinished = true;
        currentGameState = gameState.QUESTION_FINISHED;
        StartCompetition.DisableAllInteractions();

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
                        // Final competition logic
                        if (data.correct) {
                            StartCompetition.showCorrectAnswerFeedback();
                            correctAnswerSound.play();
                            $('#goodModal').modal('show');
                        } else {
                            StartCompetition.showWrongAnswerFeedback();
                            $('#notCorrectModal').modal('show');
                            InCorrectAnswerSound.play();
                        }

                        setTimeout(function () {
                            $('#goodModal, #notCorrectModal').modal('hide');

                            if (data.reset) {
                                StartCompetition.ResetPlayed();
                                CityMallFullQuestion = false;
                                OtherPlayerFullQuestion = false;
                            } else {
                                if (data.isCityMallFullQuestion) {
                                    CityMallFullQuestion = true;
                                } else {
                                    CityMallPlayed = [];
                                }
                                if (data.isOtherPlayerFullQuestion) {
                                    OtherPlayerFullQuestion = true;
                                } else {
                                    OtherTeamPlayed = [];
                                }
                            }

                            if (data.isScoreView) {
                                $('#dv-viewScore').find('#spn-lbl-goToScore').html(globalResources.ViewScores);
                                $('#dv-viewScore').css('visibility', 'visible');
                                $('#dv-viewScore').off('click').on('click', function () {
                                    $('#dv-partial').html(data.scorePartial);
                                    StartCompetition.FinishFinalCompetition();
                                });
                            }

                            $('#dv-homePage').css('visibility', 'visible');
                            $('#dv-homePage').off('click').on('click', function () {
                                $('#dv-partial').html(data.partial);
                                StartCompetition.OnLoad();
                            });
                        }, 3000);
                    }
                    else {
                        // Regular competition logic
                        if (data.correct) {
                            // Correct answer - question is finished regardless of which player
                            StartCompetition.showCorrectAnswerFeedback();
                            correctAnswerSound.play();
                            $('#goodModal').modal('show');

                            setTimeout(function () {
                                $('#goodModal').modal('hide');
                                if (data.reset) {
                                    StartCompetition.ResetPlayed();
                                }

                                // Handle navigation buttons
                                if (!data.finished && !data.continueRound) {
                                    $('#dv-homePage').css('visibility', 'visible');
                                    $('#dv-homePage').off('click').on('click', function () {
                                        $('#dv-partial').html(data.partial);
                                        StartCompetition.OnLoad();
                                    });
                                } else {
                                    if (data.finished) {
                                        $('#dv-homePage').find('#spn-lbl-goTo').html(globalResources.ViewScores);
                                    } else if (data.continueRound) {
                                        $('#dv-homePage').find('#spn-lbl-goTo').html(data.roundText);
                                    }

                                    $('#dv-homePage').css('visibility', 'visible');
                                    $('#dv-homePage').off('click').on('click', function () {
                                        if (data.continueRound && data.reset) {
                                            StartCompetition.ResetPlayed();
                                        }
                                        $('#dv-partial').html(data.partial);
                                        StartCompetition.OnLoad();
                                    });
                                }

                                if (data.isScoreView) {
                                    $('#dv-viewScore').find('#spn-lbl-goToScore').html(globalResources.ViewScores);
                                    $('#dv-viewScore').css('visibility', 'visible');
                                    $('#dv-viewScore').off('click').on('click', function () {
                                        if (data.isFinalComp) {
                                            $('#dv-partial').html(data.scorePartial);
                                            StartCompetition.FinishFinalCompetition();
                                        } else {
                                            if ($('#dv-round-scores-modal').length === 0) {
                                                $('body').append('<div id="dv-round-scores-modal"></div>');
                                            }
                                            $('#dv-round-scores-modal').html(data.scorePartial);
                                            $('#roundScoresModal').fadeIn(300);
                                            $('body').addClass('modal-open');
                                        }
                                    });
                                }
                            }, 4000);
                        }
                        else {
                            // Wrong answer - check if this is the second player
                            StartCompetition.showWrongAnswerFeedback();
                            $('#notCorrectModal').modal('show');
                            InCorrectAnswerSound.play();

                            setTimeout(function () {
                                $('#notCorrectModal').modal('hide');
                            }, 4000);

                            // Check if both players have now answered
                            if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {
                                // Both players answered (second player wrong) - question finished
                                if (data.finished) {
                                    $('#dv-homePage').find('#spn-lbl-goTo').html(globalResources.ViewScores);
                                } else if (data.continueRound) {
                                    $('#dv-homePage').find('#spn-lbl-goTo').html(data.roundText);
                                }

                                $('#dv-homePage').css('visibility', 'visible');
                                $('#dv-homePage').off('click').on('click', function () {
                                    $('#dv-partial').html(data.partial);
                                    StartCompetition.OnLoad();
                                    if (data.reset) {
                                        StartCompetition.ResetPlayed();
                                    }
                                });

                                if (data.isScoreView) {
                                    $('#dv-viewScore').find('#spn-lbl-goToScore').html(globalResources.ViewScores);
                                    $('#dv-viewScore').css('visibility', 'visible');
                                    $('#dv-viewScore').off('click').on('click', function () {
                                        if (data.isFinalComp) {
                                            $('#dv-partial').html(data.scorePartial);
                                            StartCompetition.FinishFinalCompetition();
                                        } else {
                                            if ($('#dv-round-scores-modal').length === 0) {
                                                $('body').append('<div id="dv-round-scores-modal"></div>');
                                            }
                                            $('#dv-round-scores-modal').html(data.scorePartial);
                                            $('#roundScoresModal').fadeIn(300);
                                            $('body').addClass('modal-open');
                                        }
                                    });
                                }
                            } else {
                                // First player wrong - allow second player to answer
                                isQuestionFinished = false;
                                currentGameState = gameState.QUESTION_SHOWN;

                                // Mark the first player as answered (visual feedback)
                                var firstPlayerId = playerTimer.IsCityMallPlayer ? 'dv-cityMall-player' : 'dv-Other-player';
                                StartCompetition.MarkPlayerAsAnswered(firstPlayerId);

                                selectedPlayerId = null;

                                // Re-enable player selection and keep options visible
                                $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'auto').removeClass('disabled');

                                // Clear selection, keep options visible
                                $('.answer-card').removeClass('selected correct-answer wrong-answer');
                                $('input[name="investmentExp"]').prop('checked', false).prop('disabled', false);
                                $('.img-answer-options').removeClass('bg-answer-border');

                                // Make sure answer options are clickable for second player
                                $('.answer-card, .img-answer-options').css('pointer-events', 'auto').removeClass('disabled');

                                // Reset timer display
                                $('#dv-p-timer').addClass('d-none');

                                // Show continue button and change question button
                                $('#dv-confirm').css('visibility', 'visible');
                                $('#dv-change-question').css('visibility', 'visible');

                                // Update button and re-enable change question
                                StartCompetition.UpdateContinueButtonText();
                                $('#btn-answer').prop('disabled', true).addClass('disabled');
                                $('#btn-change-question').prop('disabled', false).removeClass('disabled');
                            }
                        }
                    }

                    // Clear selections after feedback
                    setTimeout(() => {
                        document.querySelectorAll('.answer-card').forEach(card => {
                            card.classList.remove('selected', 'correct-answer', 'wrong-answer');
                        });
                        document.querySelectorAll('input[name="investmentExp"]').forEach(radio => {
                            radio.checked = false;
                        });
                    }, 1000);
                } else {
                    GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
                }
            },
            error: function (e) {
                GeneralClass.ShowErrorAlert(globalResources.ErrorOccurred);
            }
        });
    },
    DisableAllInteractions: function () {
        // Disable player selection
        $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'none').addClass('disabled');

        // Disable answer options
        $('.answer-card, .img-answer-options').css('pointer-events', 'none').addClass('disabled');
        $('input[name="investmentExp"]').prop('disabled', true);

        // Disable continue button but keep it visible
        $('#btn-answer').prop('disabled', true).addClass('disabled');

        // Disable change question button but keep it visible
        $('#btn-change-question').prop('disabled', true).addClass('disabled');
    },
    ResetObjects: function () {
        playerTimer = {
            'IsCityMallPlayer': '',
            'PlayerId': '',
            'IsTimerFinished': false,
            'IsTimerStarted': false,
            'IsFirstTime': true,
            'EndTime': 0,
            'IsPlayerAnswered': false,
            'IsPlayerCityMallAnswered': false,
            'IsOtherPlayerAnswered': false,
            'IsTimerFinishedAfterAnswer': false,
            'TimerInterval': null
        };

        // Reset game state
        currentGameState = gameState.INITIAL;
        selectedPlayerId = null;
        isQuestionFinished = false;

        // Reset visual indicators
        $('#dv-cityMall-player, #dv-Other-player').removeClass('player-answered').css('opacity', '1');

        // Re-enable interactions
        $('#dv-cityMall-player, #dv-Other-player').css('pointer-events', 'auto').removeClass('disabled');
        $('.answer-card, .img-answer-options').css('pointer-events', 'auto').removeClass('disabled');
        $('input[name="investmentExp"]').prop('disabled', false);
        StartCompetition.UpdateContinueButtonText();
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

        // Add smooth animations on load
        setTimeout(function () {
            $('.team-score-section').each(function (index) {
                $(this).css('animation-delay', (index * 0.2) + 's');
            });
        }, 100);
    },
    ShowPlayerDetails: function (playerId, isCityMall) {
        // You can implement a modal or tooltip showing more player stats
        StartCompetition.GetModalDetails(playerId, isCityMall);
    },
    GoToHomePage: function () {
        $("#loading").css("display", "flex");
        $('#full-score-container, #dv-partial').fadeOut(1500, function () {
            window.location.href = publicURls.Logout;
        });
    },

    ShowDetailedScores: function () {
        $('.full-score-container').addClass('transitioning');

        setTimeout(function () {
            StartCompetition.GetTeamScoreDetails(true);
        }, 300);
    },

    AnimateScoreReveal: function () {
        // Animate score numbers counting up
        $('.competition-team-score').each(function () {
            var $this = $(this);
            var finalScore = parseInt($this.text());
            var currentScore = 0;
            var increment = Math.ceil(finalScore / 50);

            var counter = setInterval(function () {
                currentScore += increment;
                if (currentScore >= finalScore) {
                    currentScore = finalScore;
                    clearInterval(counter);
                }
                $this.text(currentScore);
            }, 50);
        });
    },
    GetTeamScoreDetails: function (isCityMall) {
        // Add fade transition
        $('#dv-partial').fadeOut(300, function () {
            // Original AJAX code here...
            $.ajax({
                type: 'GET',
                url: publicURls.GetScoreDetails,
                dataType: 'json',
                success: function (data) {
                    if (data.isSuccess) {
                        if (data.partial != '') {
                            $('#dv-partial').html(data.partial).fadeIn(500);

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
    CloseRoundScoresModal: function () {
        $('#roundScoresModal').fadeOut(300, function () {
            $('#dv-round-scores-modal').empty();
            $('body').removeClass('modal-open');
        });
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

