import { execSync } from 'child_process';

/**
 * @returns Current branch name
 */
export function getBranchName(): string {
  return execSync('git branch --show-current', { stdio: 'pipe' }).toString().trim();
}

/**
 * @returns Last commit
 */
export function getLastCommit(): string {
  return execSync('git rev-parse HEAD', { stdio: 'pipe' }).toString().trim();
}

/**
 * @returns Current repository url
 */
export function getRepositoryUrl(): string {
  return execSync('git config --get remote.origin.url', { stdio: 'pipe' }).toString().trim();
}