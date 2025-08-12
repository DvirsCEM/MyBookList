import { send, UrlSearchParams } from "../clientUtilities";
import { createBanner } from "../components/banner";
import { getUserId } from "../tools/funcs";
import { Book } from "../tools/types";

var { bookId } = UrlSearchParams;
var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;
var coverImg = document.querySelector<HTMLImageElement>("#coverImg")!;
var titleH1 = document.querySelector<HTMLDivElement>("#titleH1")!;
var authorA = document.querySelector<HTMLAnchorElement>("#authorA")!;
var uploaderA = document.querySelector<HTMLAnchorElement>("#uploaderA")!;
var descriptionDiv = document.querySelector<HTMLDivElement>("#descriptionDiv")!;

bannerDiv.append(await createBanner(userId));

var book = await send<Book>("getBook", parseInt(bookId));

console.log(book);

coverImg.src = book.image;
titleH1.textContent = book.title;
authorA.textContent = book.author.name;
authorA.href = `/website/pages/author.html?authorId=${book.author.id}`;
uploaderA.textContent = book.uploader.username;
uploaderA.href = `/website/pages/profile.html?username=${book.uploader.username}`;
descriptionDiv.textContent = book.description;