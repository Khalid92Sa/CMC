var width = $(window).width();
var isMobile = width < 1024;


var Grid = {
    currentPageIndex: 1,
    totalPagesBeforDelete: 0,
    totalPagesBeforAdd: 0,
    visiblePages: width < 1024 ? 1 : 5,
    dtGrid: null,
    dtrGrid: null,

    refrach_grid: function (dataSource) {
        dtGrid.data = dataSource;
        dtGrid.ajax.reload();
    },
    fillGrid: function (gridId, dataSource, Columns, pagination, arraybuttons, pagerId, totalRecords, pagesize, className, params, exportURL) {
        try {
            var buttons = [];
            if (pagination === null || pagination === true) {
                var totalPages = Math.ceil(totalRecords / pagesize);
                var objForClass = eval("(" + className + ")");
                if (Grid.totalPagesBeforDelete > totalPages) {
                    Grid.currentPageIndex = Grid.currentPageIndex - 1;
                    if (Grid.currentPageIndex === 0)
                        Grid.currentPageIndex = 1;
                   objForClass.getAllWithPager(Grid.currentPageIndex, GeneralClass.pageSize);

                     Grid.totalPagesBeforDelete = 0;
                }
                if (Grid.totalPagesBeforAdd < totalPages && Grid.totalPagesBeforAdd > 0 && Grid.currentPageIndex < totalPages) {
                    Grid.currentPageIndex = Grid.currentPageIndex + 1;
                    objForClass.getAllWithPager(Grid.currentPageIndex, GeneralClass.pageSize);

                     Grid.totalPagesBeforAdd = 0;
                }
                Grid.totalPagesBeforDelete = 0;
                Grid.totalPagesBeforAdd = 0;
                Grid.apply_paginationGrid(pagerId, totalPages, pagesize, className, buttons, params);
            }

               if(this.dtGrid!=null){
               this.dtGrid.clear().rows.add(dataSource).draw();
               }else{
             this.dtGrid = $(gridId).DataTable({
                 data: dataSource,
                bLengthChange: true,
                language: {
                    "url": IsArabic ? "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json" : "//cdn.datatables.net/plug-ins/1.10.20/i18n/English.json"
                },
                processing: true,
                dom: 'lBfrtip',
                destroy: true,
                serverSide: false,
                //buttons: ['copy', 'excel', 'pdf', 'print'],
                buttons: buttons,
                /*paging: true,*/
                paging: false,
                bInfo: false,
                columns: Columns,
                lengthMenu: [[5, 10, 50, -1], [5, 10, 50, "All"]],
                iDisplayLength: pagesize,
                searching: false,
                pageLength: pagesize,
                stateSave: true,
                 "bSort": false,
                 responsive: true,

                 columnDefs: [{
                     "defaultContent": "-",
                     "targets": "_all"
                 }]

              //  aoColumns: [{ "bSortable": false }]
              //  order: true
            });
            }
            return this.dtGrid;
         /*   Grid.AddClass(gridId);*/
        }
        catch (e) {
            console.log(e);
        }
    },

    fillRowGrid: function (gridId, dataSource, Columns) {
        try {
            $(gridId).DataTable({
                data: dataSource,
                bLengthChange: true,
                language: {
                    "url": IsArabic ? "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json" : "//cdn.datatables.net/plug-ins/1.10.20/i18n/English.json"
                },
                processing: true,
                dom: 'lBfrtip',
                destroy: true,
                serverSide: false,
                paging: false,
                bInfo: false,
                columns: Columns,
                lengthMenu: [[5, 10, 50, -1], [5, 10, 50, "All"]],
                searching: false,
                stateSave: true,
                "bSort": false,
                responsive: true,

                columnDefs: [{
                    "defaultContent": "-",
                    "targets": "_all"
                }]
            });
        }
        catch (e) {
            console.log(e);
        }
    },

    //AddClass: function (gridId) {
    //    $(gridId).addClass("responsive");
    //},

    apply_paginationGrid: function (pagerId, totalPages, pageSize, className, buttons, params) {
        var firstPageClick = true;
        $("#pagerInfo").text("");
        if (totalPages === 0)
            return;
        $("#pagerInfo").text(globalResources.Page + " " + Grid.currentPageIndex + " " + globalResources.Of + " " + totalPages + " " + globalResources.Pages);
        $(pagerId).twbsPagination({
            startPage: Grid.currentPageIndex,
            stateSave: true,
            //buttons: ['copy', 'excel', 'pdf', 'print'],
            buttons: buttons,
            serverSide: false,
            totalPages: totalPages,
            first: isMobile ? '&laquo;' : globalResources.First,
            prev: isMobile ? '<' : globalResources.Previous,
            next: isMobile ? '>' : globalResources.Next,
            last: isMobile ? '&raquo;' : globalResources.Last,
            visiblePages: Grid.visiblePages,
            onPageClick: function (event, page) {
                if (firstPageClick) {
                    firstPageClick = false;
                    return;
                }
                Grid.currentPageIndex = page;
                var obj = eval("(" + className + ")");
                //if(page!==1)
               
                obj.GetAllWithPager(page, pageSize);
            }
        });
    },
    fillPopupGrid: function (gridId, dataSource, Columns, pagination, arraybuttons, pagerId, totalRecords, pagesize, className, params) {
        try {

            if (pagination === null || pagination === true) {
                var totalPages = Math.ceil(totalRecords / pagesize);
                var objForClass = eval("(" + className + ")");

                Grid.totalPagesBeforDelete = 0;
                Grid.totalPagesBeforAdd = 0;
                Grid.apply_paginationPopupGrid(pagerId, totalPages, pagesize, className, params);
            }

            var PopupGrid = $(gridId).DataTable({
                data: dataSource,
                bLengthChange: true,
                language: {
                    "url": IsArabic ? "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json" : "//cdn.datatables.net/plug-ins/1.10.20/i18n/English.json"
                },
                processing: true,
                dom: 'lBfrtip',
                destroy: true,
                serverSide: false,
                //buttons: ['copy', 'excel', 'pdf', 'print'],
                buttons: buttons,
                paging: false,
                bInfo: false,
                columns: Columns,
                lengthMenu: [[5, 10, 50, -1], [5, 10, 50, "All"]],
                iDisplayLength: pagesize,
                searching: false,
                stateSave: true,
             //   order: true
             //   aoColumns: [{ "bSortable": false }]
            });
        }
        catch (e) {
            console.log(e);
        }
    },

    apply_paginationPopupGrid: function (pagerId, totalPages, pageSize, className, params) {
        $("#pagerInfoPopup").text("");
        if (totalPages === 0) {
            $(pagerId).twbsPagination('destroy');
            return;
        }
        $("#pagerInfoPopup").text(globalResources.page + " " + Grid.currentPageIndex + " " + globalResources.of + " " + totalPages + " " + globalResources.pages);
        $(pagerId).twbsPagination({
            startPage: Grid.currentPageIndex,
            stateSave: true,
            //buttons: ['copy', 'excel', 'pdf', 'print'],
            buttons: buttons,
            serverSide: false,
            totalPages: totalPages,
            first: globalResources.first,
            prev: globalResources.previous,
            next: globalResources.next,
            last: globalResources.last,
            visiblePages: Grid.visiblePages,
            onPageClick: function (event, page) {
                Grid.currentPageIndex = page;
                var obj = eval("(" + className + ")");
                var userId = $('#hdnUserId').val();
                obj.GetActivitiesByUserId(userId, page, pageSize);
            }
        });
    }


};