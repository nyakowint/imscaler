const fs = require('fs');

const PACKAGE_PATH = 'VRChatImmersiveScaler/package.json';
const INDEX_PATH = 'index.json';
const DEFAULT_REPOSITORY = 'kittynXR/imscaler';

function readJson(path) {
  return JSON.parse(fs.readFileSync(path, 'utf8'));
}

function writeJson(path, value) {
  fs.writeFileSync(path, `${JSON.stringify(value, null, 2)}\n`);
}

function getReleaseUrl(pkg) {
  const repository = process.env.GITHUB_REPOSITORY || DEFAULT_REPOSITORY;
  const zipName = `${pkg.name.replace(/_/g, '-')}-${pkg.version}.zip`;
  return `https://github.com/${repository}/releases/download/v${pkg.version}/${zipName}`;
}

const packageJson = readJson(PACKAGE_PATH);
const packageName = packageJson.name;

let listing = {
  name: 'Immersive Scaler VRChat Tools',
  author: 'kittyn cat',
  url: 'https://immersive-scaler.kittyn.cat/index.json',
  id: 'cat.kittyn.vpm',
  packages: {}
};

if (fs.existsSync(INDEX_PATH)) {
  try {
    const existingIndex = readJson(INDEX_PATH);
    listing = {
      ...listing,
      ...existingIndex,
      packages: existingIndex.packages || {}
    };
  } catch (error) {
    console.warn(`Could not parse ${INDEX_PATH}:`, error);
  }
}

const packageListing = listing.packages[packageName] || { versions: {} };
packageListing.versions = packageListing.versions || {};

packageListing.versions[packageJson.version] = {
  ...packageJson,
  url: getReleaseUrl(packageJson)
};

listing.packages[packageName] = packageListing;

writeJson(INDEX_PATH, listing);
console.log(
  `Generated ${INDEX_PATH} with ${packageName} ${packageJson.version} ` +
  `(total versions: ${Object.keys(packageListing.versions).length})`
);
