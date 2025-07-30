$(document).ready(function () {
    fetchEarningType();

    $("#openmodal").click(function () {
        $("#earning_type").modal("show");
    });

    $("#addbtn").click(function () {
        var obj = $("#earningtypeform").serialize();
        $.ajax({
            url: '/Earning/AddEarningType',
            type: 'Post',
            contentType: 'application/x-www-form-urlencoded; charset=utf8;',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("EarningType Added Successfully!!");
                fetchEarningType();
                $("#earning_type").modal("hide");
            },
            error: function () {
                alert("Something went wrong!!!");
            }
        })
    });

    function fetchEarningType() {
        $.ajax({
            url: '/Earning/FetchEarningType',
            type: 'Get',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (result, status, xhr) {
                console.log(result); // Add this to see the data returned
                var obj = '';
                $.each(result, function (index, item) {
                    console.log(item);
                    obj += "<tr>";
                    obj += "<td>" + item.earntypeId + "</td>";
                    obj += "<td>" + item.earningName + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='edit-btn' data-id='" + item.earntypeId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='delete-btn' data-id='" + item.earntypeId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
                $("#earningtypedata").html(obj);
                attachEventHandlers();
            },
            error: function () {
                alert("Something went wrong");
            }
        })
    };

    function fetchEarningTypeDetails(id) {
        $.ajax({
            url: '/Earning/FindEarningTypeDetails?id=' + id,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                if (result) {
                    console.log(result);
                    $("#editEarntypeId").val(result.earntypeId);
                    $("#editEarningName").val(result.earningName);
                    $("#editearningtype").modal("show");
                }
            },
            error: function () {
                alert("Failed to fetch EarningType details");
            }
        });
    }

    // Save Edited Department
    $("#editbtn").click(function () {
        var obj = $("#editearningtypeform").serialize();
        $.ajax({
            url: '/Earning/UpdateEarnTypeDetails',
            type: 'POST',
            data: obj,
            contentType: "application/x-www-form-urlencoded; charset=uft8;",
            success: function () {
                alert("EarningType updated successfully");
                $("#editearningtype").modal("hide");
                fetchEarningType();
            },
            error: function () {
                alert("Failed to update EarningType");
            }
        });
    });

    function deleteEarningType(id) {
        $.ajax({
            url: '/Earning/DeleteEarningType',
            type: 'POST',
            data: { id: id },
            success: function () {
                alert("EarningType deleted successfully");
                //$("#deleteModal").modal("hide");
                fetchEarningType();
            },
            error: function () {
                alert("Failed to delete EarningType");
            }
        });
    }

    function attachEventHandlers() {
        $(".edit-btn").click(function () {
            var id = $(this).data("id");
            fetchEarningTypeDetails(id);         //fetch detail and show edit model
        });

        $(".delete-btn").click(function () {
            var id = $(this).data("id");
            if (confirm("Are you sure you want to delete this EarningType?")) {
                deleteEarningType(id);
            }
        });
    }
});