import { useInfiniteQuery, useQuery } from '@tanstack/react-query';
import {
  getApiRepositoryByRepositoryIdBranchesOptions,
  getApiRepositoryByRepositoryIdCommitsInfiniteOptions,
  getApiRepositoryByRepositoryIdFileOptions,
  getApiRepositoryByRepositoryIdTreeOptions,
} from '../client/@tanstack/react-query.gen';

const COMMITS_PAGE_SIZE = 20;

export const useRepositoryBranches = (repositoryId: string) =>
  useQuery({
    ...getApiRepositoryByRepositoryIdBranchesOptions({ path: { repositoryId } }),
    enabled: Boolean(repositoryId),
  });

export const useRepositoryTree = (repositoryId: string, branch: string, path: string) =>
  useQuery({
    ...getApiRepositoryByRepositoryIdTreeOptions({
      path: { repositoryId },
      query: { branch, path },
    }),
    enabled: Boolean(repositoryId && branch),
  });

export const useRepositoryFile = (repositoryId: string, branch: string, path: string | null) =>
  useQuery({
    ...getApiRepositoryByRepositoryIdFileOptions({
      path: { repositoryId },
      query: { branch, path: path ?? '' },
    }),
    enabled: Boolean(repositoryId && branch && path),
  });

export const useRepositoryCommits = (repositoryId: string, branch: string) =>
  useInfiniteQuery({
    ...getApiRepositoryByRepositoryIdCommitsInfiniteOptions({
      path: { repositoryId },
      query: { branch, pageSize: COMMITS_PAGE_SIZE },
    }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, _allPages, lastPageParam) =>
      lastPage.hasMore ? Number(lastPageParam) + 1 : undefined,
    enabled: Boolean(repositoryId && branch),
  });
