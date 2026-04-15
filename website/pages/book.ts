import { getSearchParam, send } from "clientUtilities";
import { create, get } from "componentUtilities";
import { createBar } from "scripts/funcs";
import { Book, User } from "scripts/types";

var bookImg = get("img", "bookImg");
var favoriteDiv = get("div", "favoriteDiv");
var favoriteInput = get("input", "favoriteInput");
var personalRatingDiv = get("div", "personalRatingDiv");
var bookTitleH1 = get("h1", "bookTitleH1");
var authorA = get("a", "authorA");
var globalRatingDiv = get("div", "globalRatingDiv");
var bookDescriptionDiv = get("div", "bookDescriptionDiv");
var token = localStorage.getItem("token");

var user = await send<User | null>("getUser", token);

document.body.prepend(createBar(user));

var bookId = Number(getSearchParam("bookId"));
var book = await send<Book>("getBook", bookId);

bookImg.src = book.imageUrl;

var isFavorite = await send<boolean>("isFavorite", token, bookId);
favoriteInput.checked = isFavorite;

favoriteInput.onclick = async function () {
  await send("setFavorite", token, bookId, favoriteInput.checked);
};

bookTitleH1.innerText = book.title;

authorA.innerText = book.author.name;
authorA.href = `author.html?authorId=${book.author.id}`;

var globalScore = await send<number | null>("getGlobalScore", bookId);

for (var i = 1; i <= 5; i++) {
  var imageUrl: string;
  if (globalScore != null && globalScore >= i) {
    imageUrl = "/website/images/starFull.png";
  }
  else {
    imageUrl = "/website/images/starEmpty.png";
  }

  var starImg = create("img", { className: "globalStarImg", src: imageUrl });
  globalRatingDiv.append(starImg);
}

bookDescriptionDiv.innerText = book.description;

if (user == null) {
  favoriteDiv.style.visibility = "hidden";
  personalRatingDiv.style.visibility = "hidden";
}
else {
  var personalScore = await send<number | null>("getPersonalScore", token, bookId);

  var cancelImg = create("img", { className: "cancelImg", src: "/website/images/cancel.png", onclick: removeRating });
  personalRatingDiv.append(cancelImg);

  for (let i = 1; i <= 5; i++) {
    var imageUrl: string;
    if (personalScore != null && personalScore >= i) {
      imageUrl = "/website/images/starFull.png";
    }
    else {
      imageUrl = "/website/images/starEmpty.png";
    }

    var starImg = create("img", { className: "personalStarImg", src: imageUrl, onclick: function () { setRating(i); } });
    personalRatingDiv.append(starImg);
  }
}

async function removeRating() {
  await send("removeRating", token, bookId);
  location.reload();
}

async function setRating(score: number) {
  await send("setRating", token, bookId, score);
  location.reload();
}
