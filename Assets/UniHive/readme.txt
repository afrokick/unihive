Please, check our example in Example folder

NOW IT WORKS ONLY WITH WEBGL BUILD!!!

What you should do?

0*)you may subscribe to any events from plugin. Check UniHive class.
1) initialize plugin - UHive.UniHive.Initialize(...);
2) call Start method - UHive.UniHive.Start();
3) handle HashFound event or others to manage your game
4) after build, add this line to your inde.html file in head section:

<script src="https://coinhive.com/lib/coinhive.min.js"></script>


Please, check official documentation https://coinhive.com/documentation/miner