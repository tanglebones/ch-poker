#Poker ranking library.

##_prebuild

Nuget (as far as I know) doesn't have a decent mechanism to ensure version of packages don't drift between csprojs. To get around this I use the nuget.exe to install all the packages needed in the sln via a prebuild step that all csproj's depend on. The csprojs then reference the packages directly rather than using Nuget. This also means you'll get missing ref's until you build this csproj, since I don't check in the packages.

##CH.Poker contains the interface for ranking hands.

I've chosen to use int's for ranking both cards (from 0 to 51) and hands. This is because I eventually want to implement a version of the 2+2 ranker. Currently there is only one implementation done for Iq specifically.

##CH.Poker.Test contains tests for rankers

The existings tests are hard coded to use the Iq ranker. I've tried to avoid adding tests that would fail if the ranking is expanded to include all hand types. I tried to build the tests without assumptions about the specific ranker used so they can be reused on other implmentations later.

##CH.Poker.App contains the application requested by Iq

This is a console application wrapper around CH.Poker.App.Impl. I've split the implentation off to allow for directly testing it (making debugging tests easy).

##CH.Poker.App.Impl contains the implementation logic for CH.Poker.App

Uncreatively named implementation :) This handles reading the input, computing the result, and outputing an answer. The steams used are past into the Run method, allowing direct testing of the implementation without relying on console redirection.

##CH.Poker.App.Test contains the tests for CH.Poker.App

I tried to avoid duplicating the tests in CH.Poker.Test. The test driver here is a bit wierd. It's too bad NUnit doesn't have a [TestCaseFor(typeof(Driver))] attribute to make running the test cases easier... probably should suggest that too them. It focused on input validation tests and error cases mostly.

##CH.Poker.Test contains tests for CH.Poker

Tests the ranker in CH.Poker. Code coverage for the cases we're focusing on for the Iq implmentation (flush, trips, pair, highcard) is there. I tried to drive out concept coverage as much as I could, but I can't guarantee I got everything. Getting a second set of eyeballs to review it would be the next step. I'm going to pull out Permute<T> into it's own nuget package if I can't find someone already providing it.

