import { createBanner } from "../components/banner";
import { getUserId } from "../tools/funcs";

var userId = await getUserId();

var bannerDiv = document.querySelector<HTMLDivElement>("#bannerDiv")!;

bannerDiv.append(await createBanner(userId));