mergeInto(LibraryManager.library, {
  uniHiveMiner: null,
  CreateMiner: function (userName, siteKey, throttle, threads) {
    try {
      userName = Pointer_stringify(userName);
      siteKey = Pointer_stringify(siteKey);

      //create options
      var opts = {
        throttle: throttle
      };

      if (threads > 0)
        opts.threads = threads;

      //create miner
      if (userName && userName.length > 0) {
        this.uniHiveMiner = CoinHive.User(siteKey, userName, opts);
      } else {
        this.uniHiveMiner = CoinHive.Anonymous(siteKey, opts);
      }

      //subscribe to events
      var goName = "UniHiveManager";
      
      this.uniHiveMiner.on('open', function () {
        SendMessage(goName, 'OnConnectionOpened');
      });

      this.uniHiveMiner.on('authed', function () {
        SendMessage(goName, 'OnAuthed');
      });

      this.uniHiveMiner.on('close', function () {
        SendMessage(goName, 'OnConnectionClosed');
      });

      this.uniHiveMiner.on('error', function (params) {
        if (params.error !== 'connection_error') {
          SendMessage(goName, 'OnErrorOccurred', 'miner:' + params.error);
        }
      });

      this.uniHiveMiner.on('job', function () {
        SendMessage(goName, 'OnNewJobReceived');
      });

      this.uniHiveMiner.on('found', function () {
        SendMessage(goName, 'OnHashFound');
      });

      this.uniHiveMiner.on('accepted', function () {
        SendMessage(goName, 'OnHashAccepted');
      });
    }
    catch (e) {
      SendMessage(goName, 'OnErrorOccurred', e.toString());
    }
  },
  StartMiner: function () {
    this.uniHiveMiner.start();
  }
  ,
  StopMiner: function () {
    this.uniHiveMiner.stop();
  },
  IsRunning: function () {
    this.uniHiveMiner.isRunning();
  },
  GetNumThreads: function () {
    return this.uniHiveMiner.getNumThreads();
  },
  GetThrottle: function () {
    return this.uniHiveMiner.getThrottle()
  },
  GetHashesPerSecond: function () {
    return this.uniHiveMiner.getHashesPerSecond();
  },
  GetTotalHashes: function () {
    return this.uniHiveMiner.getTotalHashes();
  },
  GetAcceptedHashes: function () {
    return this.uniHiveMiner.getAcceptedHashes();
  }
});