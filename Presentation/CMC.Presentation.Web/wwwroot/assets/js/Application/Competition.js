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


var CityMallPlayed = [];
var OtherTeamPlayed = [];

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

        var parsCityPlayer = parseInt(playersInCompetition.CityMallPlayer);
        var parsOtherPlayer = parseInt(playersInCompetition.OtherPlayer);
        var data = new FormData();
        data.append('CityMallPlayerId', parsCityPlayer);
        data.append('OtherPlayerId', parsOtherPlayer);

        CityMallPlayed.push(parsCityPlayer);
        OtherTeamPlayed.push(parsOtherPlayer);

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
    },
    GoToCategories: function () {
        //if (playersInCompetition.CityMallPlayer == '' && playersInCompetition.OtherPlayer == '') {
        //    return;
        //}
        //var playerId = playersInCompetition.CityMallPlayer != '' ? parseInt(playersInCompetition.CityMallPlayer) : parseInt(playersInCompetition.OtherPlayer);
        //var isCityMallPlayer = playersInCompetition.CityMallPlayer != '' ? true : false;
        //var data = new FormData();
        //data.append('playerId', playerId);
        //data.append('IsCityMallTeam', isCityMallPlayer);


        $.ajax({
            type: 'GET',
            url: publicURls.GetCategories,
            dataType: 'json',
            //data: data,
            //contentType: false,
            //processData: false,
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

                        $('#dv-partial').html(data.partial);

                        $('#btn-answer').on('click', function (e) {
                            StartCompetition.AnswerOnQuestion();
                        });

                        StartCompetition.ResetObjects();

                        $('#dv-cityMall-player, #dv-Other-player').on('click', function () {
                            var id = $(this).attr('id');
                            StartCompetition.StartTimeForPlayer(id);
                        });

                        setTimeout(function () {
                            $('#dv-question-text').removeClass('d-none');
                            $('#card-main').removeAttr('style');

                            var delay = 1500;
                            var counter = 1;
                            var totalElements = $('#dv-answers-options').find('.col-6').length;
                            var completedElements = 0;

                            $('#dv-answers-options').find('.col-6').each(function () {
                                var element = this;
                                setTimeout(function () {
                                    $(element).removeAttr('style');
                                    completedElements++;
                                    //if (completedElements === totalElements) {
                                    //    $('#dv-confirm').removeAttr('style');
                                    //}
                                }, counter * delay);
                                counter++;
                            });
                        }, 2000);
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
        if (playerTimer.IsTimerStarted == true && playerTimer.IsTimerFinished != true) {
            return;
        }

        // Prevent to re-click on same user who answered
        var checkingPlayerId = $('#' + id).find('.hdn-cPlayer').val();
        if (checkingPlayerId == playerTimer.PlayerId) {
            return;
        }

        // Both players answeres
        if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {
            return;
        }

        if (id == 'dv-cityMall-player') {
            $('#dv-other-player-timer').find('.dv-full-timer').remove();
            $('#dv-citymall-player-timer').append(timerDiv);
            playerTimer.IsCityMallPlayer = true;
            playerTimer.PlayerId = $('#' + id).find('.hdn-cPlayer').val();
        } else {
            $('#dv-citymall-player-timer').find('.dv-full-timer').remove();
            $('#dv-other-player-timer').append(timerDiv);

            playerTimer.IsCityMallPlayer = false;
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
        var questionTimer = 5000; // (parseInt($('#hdnQuestionTimer').val()) * 1000) + 1000;
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
            var player = path.animate([
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
            var pathPlayer = timer.animate([
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
                StartCompetition.AnswerOnQuestion();
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
            requestAnimationFrame(StartCompetition.PTimer);
        }
    },
    StopTimer: function () {
        setTimeout(function () {
            $('#dv-other-player-timer').find('.dv-full-timer').remove();
            $('#dv-citymall-player-timer').find('.dv-full-timer').remove();
            $('#p-timer').text('');
            $('#dv-p-timer').addClass('d-none');
        },3000);
    },
    SelectAnAnswer: function (e) {
        $('#dv-confirm').removeAttr('style');
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

        var data = new FormData();
        data.append("PlayerId", playerId);
        data.append("IsCityMallPlayer", isCityMall);
        data.append("QuestionId", questionId);
        data.append("AnswerId", answerId);
        data.append("IsCorrectAnswer", isCorrect);
        data.append("Points", questionPoints);

       
        $.ajax({
            type: 'POST',
            url: publicURls.AnswerOnQuestion,
            data: data,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.isSuccess) {
                    playerTimer.IsPlayerAnswered = true;
                    if (playerTimer.IsCityMallPlayer) {
                        playerTimer.IsPlayerCityMallAnswered = true;
                    }
                    else {
                        playerTimer.IsOtherPlayerAnswered = true;
                    }

                    if (data.correct) {
                        //Run sound effect with modal success
                        correctAnswerSound.play();
                        $('#goodModal').modal('show');
                        setTimeout(function () {
                            $('#goodModal, #notCorrectModal').modal('hide');
                            StartCompetition.OnLoad();
                            $('#dv-partial').html(data.partial);
                        }, 6000);
                    }
                    else {
                        if (playerTimer.IsTimerFinished == true) {
                            //InCase Timer Finished before player answered
                            //Host must clicked on another player.
                            timerFinishedSound.play();
                            StartCompetition.StopTimer();
                            playerTimer.IsTimerFinishedAfterAnswer = true;
                        }
                        else {
                            //Run sound effect with modal wrong
                            $('#notCorrectModal').modal('show');
                            InCorrectAnswerSound.play();
                            StartCompetition.StopTimer();
                            setTimeout(function () {
                                $('#notCorrectModal').modal('hide');
                            }, 4000);
                        }


                        if (playerTimer.IsPlayerCityMallAnswered && playerTimer.IsOtherPlayerAnswered) {
                            //Show home page button
                            $('#dv-homePage').on('click', function () {
                                StartCompetition.OnLoad();
                                $('#dv-partial').html(data.partial);
                            });

                            $('#dv-homePage').removeAttr('style');

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
    }
}