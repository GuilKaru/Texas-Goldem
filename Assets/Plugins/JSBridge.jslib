mergeInto(LibraryManager.library, {

  SendMatchRequestToJS: function () {
    var event = new CustomEvent("PokerRequestMatch", { detail: "" });
    window.dispatchEvent(event);
  },

  SendMatchEndedToJS: function () {
    var event = new CustomEvent("PokerMatchEnded", { detail: "" });
    window.dispatchEvent(event);
  }

});