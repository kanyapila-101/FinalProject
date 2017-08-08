var $loader = $("#loader");
function loadUI() {
        $.blockUI({ message: $loader.gSpinner() });
}

function unLoadUI() {
    setTimeout(function () {
        $loader.gSpinner("hide");
        $.unblockUI();
    }, 1000);
    
}

