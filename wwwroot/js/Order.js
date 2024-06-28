var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else if (url.includes("underprocess")) {
        loadDataTable("underprocess");
    }
    else if (url.includes("shipped")) {
        loadDataTable("shipped");
    }
    else if (url.includes("cancelled")) {
        loadDataTable("cancelled");
    }
    else {
        loadDataTable("all");
    }
});

function call() {
    var a = document.getElementById("orderSort");

    //$.ajax({
    //    type: 'Post',
    //    url: '/admin/Order/SetTempData',
    //    data: 'value='+a.value,
        
    //    success: function () {
    //        alert('Temp Update');
    //    }
    //});
    
    if (a.value == "pending") {
        var button = document.getElementById("pending").click();
        a.options.selectedIndex = 5;
    }
    else if (a.value == "approved") {
        var button = document.getElementById("approved").click();
    }
    else if (a.value == "underprocess") {
        var button = document.getElementById("underprocess").click();
    }
    else if (a.value == "shipped") {
        var button = document.getElementById("shipped").click();
    }
    else if (a.value == "cancelled") {
        var button = document.getElementById("cancelled").click();
    }
    else {
        var button = document.getElementById("all").click();
    }
}


function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/Order/AllOrders?status=' + status },
        "columns": [
            { data: 'name' },
            { data: 'phone' },
            { data: 'orderStatus' },
            { data: 'orderTotal' },
            { data: 'dateOfOrder' },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/order/orderDetails?id=${data}" class="btn-outline-info btn"> <i class="fa-solid fa-eye"></i> View</a> 
                    </div>`
                }
            }
        ]
    });
}

