$(document).ready(function () {
    fetchDeductionType();

    $("#openmodal").click(function () {
        $("#deduction_type").modal("show");
    });

    $("#addbtn").click(function () {
        var obj = $("#deductiontypeform").serialize();
        $.ajax({
            url: '/Deduction/AddDeductionType',
            type: 'Post',
            contentType: 'application/x-www-form-urlencoded; charset=utf8;',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("DeductionType Added Successfully!!");
                fetchDeductionType();
                $("#deduction_type").modal("hide");
            },
            error: function () {
                alert("Something went wrong!!!");
            }
        })
    });

    function fetchDeductionType() {
        $.ajax({
            url: '/Deduction/FetchDeductionType',
            type: 'Get',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (result, status, xhr) {
                console.log(result); // Add this to see the data returned
                var obj = '';
                $.each(result, function (index, item) {
                    console.log(item);
                    obj += "<tr>";
                    obj += "<td>" + item.deductionTypeId + "</td>";
                    obj += "<td>" + item.deductionsName + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='edit-btn' data-id='" + item.deductionTypeId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='delete-btn' data-id='" + item.deductionTypeId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
                $("#deductiontypedata").html(obj);
                attachEventHandlers();
            },
            error: function () {
                alert("Something went wrong");
            }
        })
    };

    function fetchdeductionTypeDetails(id) {
        $.ajax({
            url: '/Deduction/FetchDeductionTypeDetails?id=' + id,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                if (result) {
                    console.log(result);
                    $("#editDeductTypeId").val(result.deductionTypeId);
                    $("#editDeductionsName").val(result.deductionsName);
                    $("#editdeductiontype").modal("show");
                }
            },
            error: function () {
                alert("Failed to fetch DeductionsType details");
            }
        });
    }

    // Save Edited Department
    $("#editbtn").click(function () {
        var obj = $("#editdeductiontypeform").serialize();
        $.ajax({
            url: '/Deduction/UpdateDeductTypeDetails',
            type: 'POST',
            data: obj,
            contentType: "application/x-www-form-urlencoded; charset=uft8;",
            success: function () {
                alert("DeductionType updated successfully");
                $("#editdeductiontype").modal("hide");
                fetchDeductionType();
            },
            error: function () {
                alert("Failed to update DeductionType");
            }
        });
    });

    function deletedeductionType(id) {
        $.ajax({
            url: '/Deduction/DeleteDeductionType',
            type: 'POST',
            data: { id: id },
            success: function () {
                alert("DeductionType deleted successfully");

                fetchDeductionType();
            },
            error: function () {
                alert("Failed to delete DeductionType");
            }
        });
    }

    function attachEventHandlers() {
        $(".edit-btn").click(function () {
            var id = $(this).data("id");
            fetchdeductionTypeDetails(id);         //fetch detail and show edit model
        });

        $(".delete-btn").click(function () {
            var id = $(this).data("id");
            if (confirm("Are you sure you want to delete this DeductionType?")) {
                deletedeductionType(id);
            }
        });
    }
});