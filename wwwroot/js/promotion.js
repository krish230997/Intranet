$(document).ready(function () {
    // Initialize DataTable with pagination settings
    var table = $('#promotionTable').DataTable({
        "paging": true,          // Enable pagination
        "lengthMenu": [10, 25, 50, 100], // Dropdown for number of entries per page
        "pageLength": 10,        // Default entries per page
        "ordering": true,        // Enable sorting
        "searching": true,       // Enable search
        "info": true             // Show "Showing X of Y entries" info
    });

    // Fetch promotions and populate the table
    function fetchPromotions() {
        $.ajax({
            url: '/Promotion/GetPromotions',
            type: 'GET',
            success: function (data) {
                table.clear().draw(); // Clear table before appending new data

                data.forEach(item => {
                    table.row.add([
                        `<img src='/${item.profilePicture}' class="rounded-circle" style="width: 40px; height: 40px;"/> ${item.employeeName}`,
                        item.designationFrom,
                        item.designationTo,
                        item.promotionDate,
                        `<a href='#' class='editPromotion' data-id='${item.promotionId}'><i class='ti ti-edit'></i></a>
                         <a href='#' class='deletePromotion' data-id='${item.promotionId}'><i class='bi bi-trash'></i></a>`
                    ]).draw(false);
                });
            }
        });
    }

    // Open Add Promotion Modal
    $("#addPromotionButton").on("click", function () {
        $.get('/Promotion/AddPromotion', function (partialView) {
            $("#addPromotionPartial").html(partialView);
            $("#addPromotionModal").modal("show");
        });
    });

    // Submit Promotion Form
    $(document).on("submit", "#promotionForm", function (e) {
        e.preventDefault();
        const formData = $(this).serialize();
        $.post('/Promotion/AddPromotion', formData, function (response) {
            if (response.success) {
                $("#addPromotionModal").modal("hide");
                alert("Promotion Added Successfully!!!");
                fetchPromotions();
            } else {
                alert(response.message);
            }
        });
    });

    // Delete Promotion
    $(document).on("click", ".deletePromotion", function () {
        const id = $(this).data("id");
        if (confirm("Are you sure you want to delete this promotion?")) {
            $.post('/Promotion/DeletePromotion', { id }, function (response) {
                if (response.success) {
                    fetchPromotions();
                } else {
                    alert(response.message);
                }
            });
        }
    });

    // Open Edit Promotion Modal
    $(document).on("click", ".editPromotion", function () {
        const id = $(this).data("id");
        $.get(`/Promotion/EditPromotion/${id}`, function (partialView) {
            $("#editPromotionPartial").html(partialView);
            $("#editPromotionModal").modal("show");
        });
    });

    // Submit Edit Promotion Form
    $(document).on("submit", "#editPromotionForm", function (e) {
        e.preventDefault();
        const formData = $(this).serialize();
        $.post('/Promotion/EditPromotion', formData, function (response) {
            if (response.success) {
                $("#editPromotionModal").modal("hide");
                alert("Promotion Updated Successfully!!!");
                fetchPromotions();
            } else {
                alert(response.message);
            }
        });
    });

    // Initial fetch
    fetchPromotions();
});
