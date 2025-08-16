import { send, urlSearchParams } from "../clientUtilities";
import { createBanner } from "../components/banner";
import { createShelf } from "../components/shelf";
import { getUserId } from "../tools/funcs";
import { Book } from "../tools/types";

var username = urlSearchParams["username"];
console.log(username);
var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;
var usernameH1 = document.querySelector<HTMLHeadingElement>("#usernameH1")!;
var contentDiv = document.querySelector<HTMLDivElement>("#contentDiv")!;

bannerDiv.append(await createBanner(userId));

usernameH1.append(username + "'s Profile");

var [uploadedBooks, favoriteBooks] = await send<[Book[], Book[]]>("getUserBooks", username);

// console.log(uploadedBooks, favoriteBooks);

contentDiv.append(
  createShelf("Uploaded Books", uploadedBooks),
  createShelf("Favorite Books", favoriteBooks),
);