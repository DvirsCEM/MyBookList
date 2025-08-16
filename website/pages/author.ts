import { send, urlSearchParams } from "../clientUtilities";
import { createBanner } from "../components/banner";
import { createShelf } from "../components/shelf";
import { getUserId } from "../tools/funcs";
import { Book } from "../tools/types";

var authorId = parseInt(urlSearchParams["authorId"]);

var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;
var authorNameH1 = document.querySelector("#authorNameH1")!;
var contentDiv =  document.querySelector("#contentDiv")!;

bannerDiv.append(await createBanner(userId));

var [authorName, books] = await send<[string, Book[]]>("getAuthorInfo", authorId);

authorNameH1.append("Books written by " + authorName);

contentDiv.append(
  createShelf("", books),
);