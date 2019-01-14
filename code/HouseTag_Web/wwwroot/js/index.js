
var hasTag = false;
$(function () {
    var is_mobile = false;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        is_mobile = true;
    }
    if (is_mobile) {
        //手机端将一些外观设置小点
        $("#tag_tb").css("height", "108px");
        $("#search_comment").css("width", "82%");
        $('legend').css("font-size", "16px");
        $('fieldset').css("margin-top", "5px");
        if (screen.height > 730) {
            $('#btn_start').css("margin-left", "10px");
        }
        else {
            $('#btn_start').css("margin-left", "5px");
        }
    }
    else {
        $("#p_info").css("width", "445px");
        $("#search_comment").css("width", "30%");
    }
});

//开始
function start() {
    var cityId = 0;
    $("#city_content").find("input").each(function (i) {
        var checked = $(this).prop("checked");
        var value = $(this).val();
        if (checked === true) {
            cityId = value;
        }
    });
    var name = $("#pName").val();
    if (name.trim().length === 0) {
        return;
    }
    clear();
    var url = "/home/GetProjectInfo/?cityId=" + cityId + "&name=" + name;
    var id = layer.msg('正在获取楼盘信息...', {
        time: 0//20s后自动关闭,
    });
    $.ajax({
        type: "get",
        url: url,
        success: function (data) {
            if (data.status === 1) {
                $("#p_name").html(data.data.name)
                //console.info(data.data.address.length);
                $("#p_address").html(data.data.address)
                let price = data.data.price;
                $("#p_price").html(price === '' ? "暂无价格" : price);
                var len = data.data.address.length;
                layer.close(id);
                getTagInfo(data.data.name);
            }
            else {
                console.info("GetProjectInfo_exception:" + data.error);
            }
        },
        complete: function (XMLHttpRequest, textStatus) {
            layer.close(id);
        }
    });
}
//获取标签信息
function getTagInfo(name) {
    var url = "/home/GetTagInfo/?name=" + name;

    var id = layer.msg('正在获取评论信息...', {
        time: 0
    });
    $.ajax({
        type: "get",
        url: url,
        success: function (data) {
            if (data.status === 1) {
                //$("#tag").empty();
                for (let i = 0; i < data.data.length; i++) {
                    let name = data.data[i].name;
                    let count = data.data[i].count;
                    let count_str = count > 99 ? "(99+)" : "(" + count + ")";
                    let span_str = "<span id='span_tag_" + i + "' class='el-tag' onClick='getComment(this); '>" + name + count_str + "</span > ";
                    $("#tag").append(span_str);
                }
                //有标签数据才显示
                if (data.data.length >= 1) {
                    //默认选中第一个
                    getComment($("#span_tag_0"));
                    hasTag = true;
                }
                else {
                    //没有标签尝试获取所有评论数据
                    searchComment(true);
                    hasTag = false;
                }

            }
            else {
                console.info("GetTagInfo_exception:" + data.error);
            }
        },
        complete: function (XMLHttpRequest, textStatus) {
            layer.close(id);
        }

    });
}

function clear() {
    $("#p_name").html("");
    $("#p_address").html("");
    $("#p_price").html("");
    $("#comment_tag").html("");
    $("#tag").empty();
    $("#comment").empty();
    $("#search_tips").hide();

}

//获取评论
window.getComment = function (obj) {
    //选择的文本
    let text = $(obj).text().split('(')[0];
    $("#comment_tag").html("[" + text + "]相关评论");
    $("#tag").find("span").each(function (i) {
        var this_text = $(this).text().split('(')[0];
        console.info("text:" + text + " this_text:" + this_text);
        if (this_text === text) {
            $(this).css("color", "red");
        } else {
            $(this).css("color", "#e6a23c");
        }
    });

    var url = "/home/GetCommentInfo/?TagName=" + text;
    $.ajax({
        type: "get",
        url: url,
        success: function (data) {
            if (data.status === 1) {
                //显示评论
                showComment(data.data);
            }
            else {
                console.info("GetTagInfo_exception:" + data.error);
            }
        }
    });


}

//显示搜索
function showSearch() {
    if ($("#search_c").is(":hidden")) {
        //显示标签搜索
        $("#search_c").show();
        $("#search_a").html('关闭搜索');
        $("#search_comment").val('');
    } else {
        //关闭标签搜索
        $("#search_c").hide();
        $("#search_a").html('评论搜索');
        //有标签时 关闭的时候默认选择第一个标签
        if (hasTag) {
            getComment($("#span_tag_0"));
        }
        else {
            //显示所有评论
            searchComment(true);
        }
    }
}

function searchComment(all = false) {
    var text = $("#search_comment").val();
    //过滤中英文字符
    text = text.replace(/[\ |\~|\`|\!|\@|\#|\$|\%|\^|\&|\*|\(|\)|\-|\_|\+|\=|\||\\|\[|\]|\{|\}|\;|\:|\"|\'|\,|\<|\.|\>|\/|\?]/g, "");
    text = text.replace(/[\ \！|\|\……\|\；|\：|\“|\，\【|\】|\《|\.|\》|\。]/g, "");
    //获取所有评论
    if (all) {
        text = "all_c";
    }
    if (text.trim().length === 0) {
        return;
    }
    var url = "/home/SearchComment/?text=" + text;
    $.ajax({
        type: "get",
        url: url,
        success: function (data) {
            if (data.status === 1) {

                if (all) {
                    if (data.data.length > 0) {
                        $("#comment_tag").html("所有评论");

                        $("#tag").html("<span class='tag_tips'>相同评论关键词太少无法提取标签</span>");

                    }
                    else {
                        $("#tag").html("<span class='tag_tips'>暂无评论</span>");
                    }
                }
                else {
                    if (data.data.length > 0) {
                        $("#comment_tag").html("[" + text + "]相关评论");
                    }
                    else {
                        $("#comment_tag").html("没有找到 [" + text + "] 相关评论");
                    }
                }

                //显示评论
                showComment(data.data);

            }
            else {
                console.info("SearchComment_exception:" + data.error);
            }
        }
    });
}

//显示评论
function showComment(data) {
    $("#comment").html('');
    if (data.length > 0) {
        //显示评论搜索提示
        $("#search_tips").show();
    }
    for (let i = 0; i < data.length; i++) {
        let author = data[i].author;
        let content = data[i].content;
        let date = data[i].date;
        date = date.split('T')[0];
        let span_str = "<span style='margin: 5px; display: block; '><b>" + author + "</b> <i>" + date + "</i></span>";
        let span_str2 = "<span style='margin:3px;display:block;'>" + content + "</span>";

        $("#comment").append(span_str + span_str2);
    }
}