$(document).ready(function () {
    fetchEarning();

    //Earning Part
    $("#openearningmodal").click(function () {
        $("#earning").modal("show");
    });

    $("#addearningbtn").click(function () {
        var obj = $("#earningform").serialize();
        $.ajax({
            url: '/Earning/AddEarning',
            type: 'Post',
            contentType: 'application/x-www-form-urlencoded; charset=utf8;',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Earning Added Successfully!!");
                fetchEarning();
                $("#earning").modal("hide");
            },
            error: function () {
                alert("Something went wrong!!!");
            }
        })
    });

    function fetchEarning() {
        $.ajax({
            url: '/Earning/FetchEarning',
            type: 'Get',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (result, status, xhr) {
                console.log(result); // Add this to see the data returned
                var obj = '';
                $.each(result, function (index, item) {
                    console.log(item);
                    obj += "<tr>";
                    obj += "<td>" + item.earningsId + "</td>";
                    obj += "<td>" + item.earningType.earningName + "</td>";
                    obj += "<td>" + item.earningsPercentage + "</td>";
                    obj += "<td>" + item.department.name + "</td>";
                    obj += "<td>" + item.designation.name + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='editearn-btn' data-id='" + item.earningsId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='deleteearn-btn' data-id='" + item.earningsId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
                $("#earningdata").html(obj);
                attachEventHandlers();
            },
            error: function () {
                alert("Something went wrong");
            }
        })
    };

    function fetchEarningDetails(id) {
        $.ajax({
            url: '/Earning/FindEarningDetails?id=' + id,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                if (result) {
                    console.log(result);
                    $("#editEarningsId").val(result.earningsId);
                    $("#editEarntypeId").val(result.earntypeId).trigger('change');
                    $("#editEarningsPercentage").val(result.earningsPercentage);
                    $("#editDepartmentId").val(result.departmentId).trigger('change');
                    $("#editDesignationId").val(result.designationId).trigger('change');
                    $("#editearning").modal("show");
                }
            },
            error: function () {
                alert("Failed to fetch Earning details");
            }
        });
    }

    // Save Edited Department
    $("#editearningbtn").click(function () {
        var obj = $("#editearningform").serialize();
        $.ajax({
            url: '/Earning/UpdateEarningDetails',
            type: 'POST',
            data: obj,
            contentType: "application/x-www-form-urlencoded; charset=uft8;",
            success: function () {
                alert("EarningType updated successfully");
                $("#editearning").modal("hide");
                fetchEarning();
            },
            error: function () {
                alert("Failed to update Earning");
            }
        });
    });

    function deleteEarning(id) {
        $.ajax({
            url: '/Earning/DeleteEarning',
            type: 'POST',
            data: { id: id },
            success: function () {
                alert("Earning deleted successfully");
                //$("#deleteModal").modal("hide");
                fetchEarning();
            },
            error: function () {
                alert("Failed to delete Earning");
            }
        });
    }

    function attachEventHandlers() {
        $(".editearn-btn").click(function () {
            var id = $(this).data("id");
            fetchEarningDetails(id);         //fetch detail and show edit model
        });

        $(".deleteearn-btn").click(function () {
            var id = $(this).data("id");
            if (confirm("Are you sure you want to delete this Earning?")) {
                deleteEarning(id);
            }
        });
    }
});