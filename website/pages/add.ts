import { send } from "clientUtilities";
import { create, get } from "componentUtilities";
import { createBar } from "scripts/funcs";
import { Author, User } from "scripts/types";

var authorOptions = get("datalist", "authorOptions");
var titleInput = get("input", "titleInput");
var authorInput = get("input", "authorInput");
var coverUrlInput = get("input", "coverUrlInput");
var descriptionTextarea = get("textarea", "descriptionTextarea");
var submitButton = get("button", "submitButton");
var errorDiv = get("div", "errorDiv");

var token = localStorage.getItem("token");
var user = await send<User | null>("getUser", token);

document.body.prepend(createBar(user));

var authors = await send<Author[]>("getAllAuthors");
for (var author of authors) {
  var authorOption = create("option", { value: author.name });
  authorOptions.append(authorOption);
}

submitButton.onclick = async function () {
  var title = titleInput.value.trim();
  var authorName = authorInput.value.trim();
  var imageUrl = coverUrlInput.value.trim();
  var description = descriptionTextarea.value.trim();

  if (title == "" || authorName == "" || imageUrl == "" || description == "") {
    errorDiv.innerText = "All fields are required.";
    return;
  }

  errorDiv.innerText = "";

  await send("addBook", title, authorName, imageUrl, description);
  location.href = "index.html";
};
