dbProfile = db.getSiblingDB('profileDatabase');
dbPosts = db.getSiblingDB('profilePosts');

// Create the 'profiles' collection and insert a sample document
dbProfile.createCollection("profiles");
dbProfile.profiles.insertOne({
    userGuid: "sample-user-guid",
    firstName: "Connor",
    lastName: "Hoffman",
    bio: "This is a sample bio."
});

// Create the 'posts' collection and insert a sample post
dbPosts.createCollection("posts");
dbPosts.posts.insertOne({
    userGuid: "sample-user-guid",
    title: "Sample Post",
    content: "This is a sample post content."
});