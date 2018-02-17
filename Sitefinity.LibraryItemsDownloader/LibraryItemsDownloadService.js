// Telerik.Sitefinity.Web.UI.RadListViewBinder
// Telerik.Sitefinity.Web.UI.RadGridBinder 
var my_binder;
var masterViewElement;
var loadingElementSelector = '.RadAjax'; 

var supportedCommands = {
    'DownloadSelectedImages': {
        downloadLink: 'DownloadImages',
        zipFileName: 'Images'
    },
    'DownloadSelectedDocuments': {
        downloadLink: 'DownloadDocuments',
        zipFileName: 'Documents'
    },
    'DownloadSelectedVideos': {
        downloadLink: 'DownloadVideos',
        zipFileName: 'Videos'
    }
}

console.log('External JS Loaded');

// called by the MasterGridView when it is loaded
// the sender here is MasterGridView / Telerik.Sitefinity.Modules.Libraries.LibrariesMasterGridView
function OnMasterViewLoadedCustom(sender, args) {

    my_binder = sender.get_binder();
    masterViewElement = sender.get_element();

    var itemsGrid = sender.get_currentItemsList();
    itemsGrid.add_command(downloadSelectedImages);

}

function showLoading() {
    $(masterViewElement).find(loadingElementSelector).show();
}

function hideLoading() {
    $(masterViewElement).find(loadingElementSelector).hide();
}

///  the sender here is Telerik.Sitefinity.Web.UI.ItemLists.ItemsList
function downloadSelectedImages(sender, args) {

    var commandName = args.get_commandName();
    if (!supportedCommands[commandName]) {
        return;
    }

    var items = my_binder.get_selectedItems();
    if (!items || items.length < 1) {
        alert('Please select items!');
    }

    var zipFileName = supportedCommands[commandName].zipFileName;

    var dataItem = sender.get_dataItem();
    if (!!dataItem && !!dataItem.LibraryTitle) {
        zipFileName = dataItem.LibraryTitle;
    }
    var imageIds = items.map(function (item) {
        return item.Id;
    });

    showLoading();
    var data = JSON.stringify(imageIds);
    var url = '/LibrariesService/' + supportedCommands[commandName].downloadLink;
    $.ajax({
        method: 'POST',
        url: url,
        data: data,
        dataType: "json",
        contentType: "application/json",
        success: function (data) {
            downloadFile(data, zipFileName);
        },
        error: function (e) {
            console.error(e);
        },
        complete: function (e) {
            hideLoading();
            deselectLibraryItems();
        }
    });
}

function deselectLibraryItems() {
    if (Telerik.Sitefinity.Web.UI.RadListViewBinder && my_binder instanceof Telerik.Sitefinity.Web.UI.RadListViewBinder) {
        my_binder.deselectAll();
    } else if (Telerik.Sitefinity.Web.UI.RadGridBinder && my_binder instanceof Telerik.Sitefinity.Web.UI.RadGridBinder ) {
        my_binder.clearSelection();
    } 
}

// https://stackoverflow.com/questions/35038884/download-file-from-bytes-in-javascript/37340749#37340749
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

function downloadFile(byteArray, zipFileName) {
    var data = base64ToArrayBuffer(byteArray);
    var blob = new Blob([data], { type: 'application/zip' });
    var link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    var fileName = zipFileName + '.zip';
    link.download = fileName;
    link.click();
}
