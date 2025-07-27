import { send } from "../clientUtilities";
import { Item } from "../types";

var nameInput = document.querySelector<HTMLInputElement>("#nameInput")!;
var priceInput = document.querySelector<HTMLInputElement>("#priceInput")!;
var addButton = document.querySelector<HTMLButtonElement>("#addButton")!;
var itemsList = document.querySelector<HTMLUListElement>("#itemsList")!;

var items = await send<Item[]>("getItems");

for (var item of items) {
  var li = document.createElement("li");
  li.textContent = `${item.name} - ₪${item.price}`;
  itemsList.append(li);
};

addButton.onclick = async () => {
  await send("addItem", nameInput.value, parseFloat(priceInput.value));
  location.reload();
};
