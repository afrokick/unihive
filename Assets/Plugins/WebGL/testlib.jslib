mergeInto(LibraryManager.library, {
  miner: null,
  CreateMiner: function (key) {
    key = Pointer_stringify(key);

    var opts = {
      //threads: 1
    };

    this.miner = CoinHive.Anonymous(key, opts);

    console.log("create miner with key:" + key);

    var selfMiner = this.miner;

    this.miner.on('accepted', function () {
      console.log('accepted');
      
      SendMessage('Logic', 'OnHashAccepted', selfMiner.getHashesPerSecond());
    });
  },
  StartMiner: function () {
    this.miner.start();
  },
  StopMiner: function () {
    this.miner.stop();
  }
});