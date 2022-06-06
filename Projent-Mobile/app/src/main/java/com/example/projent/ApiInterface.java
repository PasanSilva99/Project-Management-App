package com.example.projent;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.GET;

public interface ApiInterface {

    @GET("users")
    Call<List<RegistrationPojo>> getUsers();

    @GET("projects")
    Call<List<ProjectsPojo>> getProjects();

    @GET("messages")
    Call<List<MessagesPojo>> getMessages();

    @GET("assignees")
    Call<List<AssigneesPojo>> getAssignees();

    @GET("tasks")
    Call<List<TasksPojo>> getTasks();


}
