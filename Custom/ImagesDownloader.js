var my_binder;

// called by the MasterGridView when it is loaded
// the sender here is MasterGridView
function OnMasterViewLoadedCustom(sender, args) {

    // sender.add_itemCommand(handleMasterGridViewItemCommand);

    
    // .get_selectedItems()

    my_binder = sender.get_binder();

     var itemsGrid = sender.get_currentItemsList();
     itemsGrid.add_command(onMyCommand);
    // itemsGrid.add_beforeCommand(handleItemsGridBeforeCommand);
     // itemsGrid.add_itemCommand(onMyItemCommand);
    // itemsGrid.add_dialogClosed(handleItemsGridDialogClosed);
}

function onMyCommand(sender, args) {

    var commandName = args.get_commandName();
    if (commandName !== 'DownloadSelectedImages') {
        return;
    }

    var items = my_binder.get_selectedItems();
    if (!items || items.length < 1) {
        alert('Please select items!');
    }


    var dataItem = sender.get_dataItem();
    var zipFileName = 'Images';
    if (dataItem !== null) {
        zipFileName = dataItem.LibraryTitle;
    }
    var imageIds = items.map(function (item) {
        return item.Id;
    });

    var data = JSON.stringify(imageIds);
    $.ajax({
        method: 'POST',
        url: '/LibrariesService/DownloadImages',
        data: data,
        dataType: "json",
        contentType: "application/json",
        success: function (data) {
            suc(data, zipFileName);
        },
        error: function (e) {
            console.error(e);
        }
    });
    
}

function base64ToArrayBuffer(base64) {
    var binaryString = window.atob(base64);
    var binaryLen = binaryString.length;
    var bytes = new Uint8Array(binaryLen);
    for (var i = 0; i < binaryLen; i++) {
        var ascii = binaryString.charCodeAt(i);
        bytes[i] = ascii;
    }
    return bytes;
}

function suc(byteArray, zipFileName) {
    var data = base64ToArrayBuffer(byteArray);
    var blob = new Blob([ data ], { type: 'application/zip' });
    var link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    var fileName = zipFileName + '.zip';
    link.download = fileName;
    link.click();
}

function onMyItemCommand(sender, args) {
    console.log('OnItemCommand');
    return true;
}