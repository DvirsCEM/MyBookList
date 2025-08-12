import { send } from "../clientUtilities";
import { createBanner } from "../components/banner";
import { getUserId } from "../tools/funcs";

var userId = await getUserId();
if (userId == null) {
  location.href = "/website/pages/index.html";
}

document.querySelector<HTMLDivElement>("#bannerDiv")!
  .append(await createBanner(userId));

var titleInput = document.querySelector<HTMLInputElement>("#titleInput")!;
var authorInput = document.querySelector<HTMLInputElement>("#authorInput")!;
var coverInput = document.querySelector<HTMLInputElement>("#coverInput")!;
var descInput = document.querySelector<HTMLTextAreaElement>("#descInput")!;
var coverImg = document.querySelector<HTMLImageElement>("#coverImg")!;
var addButton = document.querySelector<HTMLButtonElement>("#addButton")!;

coverInput.onchange = () => {
  var file = coverInput.files?.[0];
  if (file == null) {
    coverImg.src = "";
    return;
  }

  var reader = new FileReader();
  reader.onload = () => {
    coverImg.src = reader.result as string;
  };
  reader.readAsDataURL(file);
};

addButton.onclick = async () => {
  var title = titleInput.value;
  var authorName = authorInput.value;
  var cover = coverImg.src;
  var description = descInput.value;

  if (title == "" || authorName == "" || cover == null || description == "") {
    alert("Please fill in all fields.");
    return;
  }

  await send("addBook", title, authorName, cover, description, userId);
};
