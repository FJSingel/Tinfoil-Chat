Kay so the way my client class works is that it'll initialize on a port, listening for connections to occur.
(Default is 420)

it'll automatically run waitForConnection on a separate thread that listens for connections

you can connect to a specific user using findUser(string ip, int port)

once a connection is established (aka socket creation was successful) that socket will be added to the list of successfully created sockets

it is the job of the user management to remember the order in which the users were found in.
we might have to modify this in a bit so that user specific information will be passed on.

right now user verification has not been added

you can use the message(int clNo, string msg) function if you remember which client was added to list in order starting from client 0 (i.e you want to send a message to the second client you added/found use: message(1,"hi");

this will also be used later on in user verification i.e the user management layer will send a message containing a header which indicates the background layer of communcation which will attempt a handshake (verification of user)

we can add a header for chat communication to avoid crossing information so something like "& asdfasdfdf" for background communication and "@ asdfasdfasdf" for chat communication


(Frank should look at this and tell me if I'm doing this so wrong)
something like:

User 1:
findUser(0.0.0.0, 420); // first user
message(0, "& " + myInfo + key.toString());

//waits for specific message including "& " as the header and the next part should be the info of the guy you want to verify + his key

//and then chat can continue
(End of pseudo-verification)


Anyways once you create a client object you need to get the MemoryStream that the client will be outputting all messages it gets from any of the clients. You'll need to poll the stream for new messages.