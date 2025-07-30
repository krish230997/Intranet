$(document).ready(function () {
    fetchDeduction();

    //Earning Part
    $("#opendeductionmodal").click(function () {
        $("#deduction").modal("show");
    });

    $("#adddeductionbtn").click(function () {
        var obj = $("#deductionform").serialize();
        $.ajax({
            url: '/Deduction/AddDeduction',
            type: 'Post',
            contentType: 'application/x-www-form-urlencoded; charset=utf8;',
            data: obj,
            dataType: 'json',
            success: function () {
                alert("Deduction Added Successfully!!");
                fetchDeduction();
                $("#deduction").modal("hide");
            },
            error: function () {
                alert("Something went wrong!!!");
            }
        })
    });

    function fetchDeduction() {
        $.ajax({
            url: '/Deduction/FetchDeduction',
            type: 'Get',
            contentType: 'application/json; charset=utf8',
            dataType: 'json',
            success: function (result, status, xhr) {
                console.log(result); // Add this to see the data returned
                var obj = '';
                $.each(result, function (index, item) {
                    console.log(item);
                    obj += "<tr>";
                    obj += "<td>" + item.deductionId + "</td>";
                    obj += "<td>" + item.deductionType.deductionsName + "</td>";
                    obj += "<td>" + item.deductionPercentage + "</td>";
                    obj += "<td>" + item.department.name + "</td>";
                    obj += "<td>" + item.designation.name + "</td>";
                    obj += "<td>";
                    obj += "<a href='#' class='editdeduct-btn' data-id='" + item.deductionId + "'><i class='ti ti-edit'></i></a> ";
                    obj += "<a href='#' class='deletededuct-btn' data-id='" + item.deductionId + "'><i class='bi bi-trash'></i></a>";
                    obj += "</td>";
                    obj += "</tr>";
                });
                $("#deductiondata").html(obj);
                attachEventHandlers();
            },
            error: function () {
                alert("Something went wrong");
            }
        })
    };

    function fetchDeductionDetails(id) {
        $.ajax({
            url: '/Deduction/FindDeductionDetails?id=' + id,
            type: 'GET',
            dataType: 'json',
            success: function (result) {
                if (result) {
                    console.log(result);
                    $("#editDeductionId").val(result.deductionId);
                    $("#editDeductTypeId").val(result.deductionTypeId).trigger('change');
                    $("#editDeductionPercentage").val(result.deductionPercentage);
                    $("#editDepartmentId").val(result.departmentId).trigger('change');
                    $("#editDesignationId").val(result.designationId).trigger('change');
                    $("#editdeduction").modal("show");
                }
            },
            error: function () {
                alert("Failed to fetch Earning details");
            }
        });
    }

    // Save Edited Department
    $("#editdeductionbtn").click(function () {
        var obj = $("#editdeductionform").serialize();
        $.ajax({
            url: '/Deduction/UpdateDeductionDetails',
            type: 'POST',
            data: obj,
            contentType: "application/x-www-form-urlencoded; charset=uft8;",
            success: function () {
                alert("Deduction updated successfully");
                $("#editdeduction").modal("hide");
                fetchDeduction();
            },
            error: function () {
                alert("Failed to update Deduction");
            }
        });
    });

    function deleteDeduction(id) {
        $.ajax({
            url: '/Deduction/DeleteDeduction',
            type: 'POST',
            data: { id: id },
            success: function () {
                alert("Deduction deleted successfully");
                //$("#deleteModal").modal("hide");
                fetchDeduction();
            },
            error: function () {
                alert("Failed to delete Deduction");
            }
        });
    }

    function attachEventHandlers() {
        $(".editdeduct-btn").click(function () {
            var id = $(this).data("id");
            fetchDeductionDetails(id);         //fetch detail and show edit model
        });

        $(".deletededuct-btn").click(function () {
            var id = $(this).data("id");
            if (confirm("Are you sure you want to delete this Deduction?")) {
                deleteDeduction(id);
            }
        });
    }
});