//2 databases: profileDatabase and profilePosts
dbProfile = db.getSiblingDB('profileDatabase');
dbPosts = db.getSiblingDB('profilePosts');
dbFriends = db.getSiblingDB('profileFriends');
dbMessages = db.getSiblingDB('messagesDB');)

//create collections
dbProfile.createCollection("profiles");
dbPosts.createCollection("posts");
dbPosts.createCollection("comments");
dbFriends.createCollection("friendRequests");
dbFriends.createCollection("friendships");
dbMessages.createCollection("messages");

//create admin user
dbAdmin = db.getSiblingDB('admin');

dbAdmin.createUser({
    user: 'root2',
    pwd: 'password',
    roles: [
        { role: 'userAdminAnyDatabase', db: 'admin' },
        'readWriteAnyDatabase',
        'dbAdminAnyDatabase',
        'clusterAdmin'
    ]
});