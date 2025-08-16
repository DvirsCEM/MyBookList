import { send, urlSearchParams } from "../clientUtilities";
import { createBanner } from "../components/banner";
import { create } from "../componentUtilities";
import { getUserId } from "../tools/funcs";
import { Book } from "../tools/types";

var bookId = parseInt(urlSearchParams["bookId"]);
var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;
var coverImg = document.querySelector<HTMLImageElement>("#coverImg")!;
var favoriteDiv = document.querySelector<HTMLDivElement>("#favoriteDiv")!;
var rateDiv = document.querySelector<HTMLDivElement>("#rateDiv")!;
var titleH1 = document.querySelector<HTMLDivElement>("#titleH1")!;
var authorA = document.querySelector<HTMLAnchorElement>("#authorA")!;
var averageDiv = document.querySelector<HTMLDivElement>("#averageDiv")!;
var uploaderA = document.querySelector<HTMLAnchorElement>("#uploaderA")!;
var descriptionDiv = document.querySelector<HTMLDivElement>("#descriptionDiv")!;

bannerDiv.append(await createBanner(userId));

var book = await send<Book>("getBook", bookId);

coverImg.src = book.image;
titleH1.textContent = book.title;
authorA.textContent = book.author.name;
authorA.href = `/website/pages/author.html?authorId=${book.author.id}`;
uploaderA.textContent = book.uploader.username;
uploaderA.href = `/website/pages/profile.html?username=${book.uploader.username}`;
descriptionDiv.textContent = book.description;

// Handle average rating stuff
// ===========================
var averageScore = await send<number>("getAverageRatingScore", bookId);

for (var i = 1; i <= 5; i++) {
  var starImgSrc = i <= averageScore ?
    `/website/images/starFull.png` : `/website/images/starEmpty.png`;
  averageDiv.append(create("img", { className: "bigStarImg", src: starImgSrc }));
}


if (userId != null) {
  // Handle favorite stuff
  // =====================
  var updateFavorite = () => {
    if (favoriteInput.checked) {
      send("addFavorite", userId, bookId);
    }
    else {
      send("removeFavorite", userId, bookId);
    }
  };

  var favoriteInput = create("input", { type: "checkbox", id: "favoriteInput", onclick: updateFavorite });

  favoriteDiv.append(
    favoriteInput,
    create("div", {}, ["Add to Favorites"]),
  );

  var isFavorite = await send<boolean>("getIsFavorite", userId, bookId);
  favoriteInput.checked = isFavorite;


  // Handle rating stuff
  // ===================
  var score = await send<number | null>("getRatingScore", userId, bookId);

  var removeRating = async () => {
    await send("removeRating", userId, bookId);
    location.reload();
  }

  rateDiv.append(
    create("button", { id: "cancelButton", className: "rateButton", onclick: removeRating }, [
      create("img", { className: "rateImg", src: "/website/images/cancel.png" })
    ])
  );

  for (let i = 1; i <= 5; i++) {
    var starImgSrc = score != null && i <= score ?
      `/website/images/starFull.png` : `/website/images/starEmpty.png`;

    var rate = async (score: number) => {
      await send("rate", score, userId, bookId);
      location.reload();
    };

    rateDiv.append(
      create("button", { className: "rateButton", onclick: () => rate(i) }, [
        create("img", { className: "rateImg", src: starImgSrc })
      ])
    );
  }
}